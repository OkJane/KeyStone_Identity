using KeyStone_Identity.Core.Interfaces;

namespace KeyStone_Identity.API.Context
{
    public class RequestContext : IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? IPAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
    }
}
