using System.Security.Claims;
using AspNetCoreRateLimit;

namespace MeuCatalogo.API.Infrastructure.RateLimiting;

public class JwtUserClientResolveContributor : IClientResolveContributor
{
    public const string AnonymousClientId = "anon";

    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        var userId = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Task.FromResult(string.IsNullOrEmpty(userId) ? AnonymousClientId : userId);
    }
}
