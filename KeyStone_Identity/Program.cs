using KeyStone_Identity.API.Context;
using KeyStone_Identity.API.Middleware;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Core.Services;
using KeyStone_Identity.Infrastructure.Data;
using KeyStone_Identity.Infrastructure.Repositories;
using KeyStone_Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting server");
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));

    // Add services to the container.
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            var httpContext = context.HttpContext;
            var response = httpContext.Response;
            response.StatusCode = StatusCodes.Status429TooManyRequests;
            response.ContentType = "application/problem+json";
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) && retryAfter > TimeSpan.Zero)
            {
                response.Headers["Retry-After"] = ((int)retryAfter.TotalSeconds).ToString();
            }

            var retryAfterSeconds = context.Lease.TryGetMetadata(
                MetadataName.RetryAfter, out var ra) && ra > TimeSpan.Zero
                ? (int?)((int)ra.TotalSeconds)
                : null;
            await response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc6585#section-4",
                title = "Too Many Requests",
                status = 429,
                detail = "You have exceeded the allowed request rate. "
                                 + "Please wait before retrying.",
                retryAfterSeconds
            }, cancellationToken: cancellationToken);

            var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RateLimiting");

            logger.LogWarning(
                "Rate limit exceeded: {Method} {Path} from {RemoteIp} — RetryAfter {RetryAfterSeconds}s",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Connection.RemoteIpAddress,
                retryAfterSeconds?.ToString() ?? "unknown");
        };

        options.AddPolicy("per-ip", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromSeconds(60),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                })
        );

        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromSeconds(60);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

        });
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddDbContext<KeyStone_Identity_DbContext>(x => x.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]));
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, JwtTokenService>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IActivationTokenRepository, ActivationTokenRepository>();
    builder.Services.AddScoped<IRequestContext, RequestContext>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
        };
    });
    builder.Services.AddAuthorization();

    var app = builder.Build();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();

    }

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseRateLimiter();

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    // Ignore host-aborted exception triggered by tooling such as EF Core.
}
catch (Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
