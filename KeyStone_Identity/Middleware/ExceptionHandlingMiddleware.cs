using KeyStone_Identity.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace KeyStone_Identity.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this._logger = logger;
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            context.Response.ContentType = "application/json";

            switch (ex)
            {
                case OidcException oidcEx:
                    context.Response.StatusCode = oidcEx.StatusCode;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        error = oidcEx.ErrorCode,
                        error_description = oidcEx.Message
                    }));
                    break;

                case DomainException domainEx:
                    context.Response.StatusCode = domainEx.StatusCode;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        status = domainEx.StatusCode,
                        error = domainEx.Message,
                        traceId = context.TraceIdentifier
                    }));
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        status = 500,
                        error = "An unexpected error occurred.",
                        traceId = context.TraceIdentifier
                    }));
                    break;
            }
        }
    }
}
