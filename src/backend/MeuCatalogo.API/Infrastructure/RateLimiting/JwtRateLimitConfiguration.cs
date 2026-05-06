using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;

namespace MeuCatalogo.API.Infrastructure.RateLimiting;

public class JwtRateLimitConfiguration : RateLimitConfiguration
{
    public JwtRateLimitConfiguration(
        IOptions<IpRateLimitOptions> ipOptions,
        IOptions<ClientRateLimitOptions> clientOptions)
        : base(ipOptions, clientOptions)
    {
    }

    public override void RegisterResolvers()
    {
        base.RegisterResolvers();
        // Resolve per-user via JWT first; fall back to header-based ClientId for service-to-service.
        ClientResolvers.Insert(0, new JwtUserClientResolveContributor());
    }
}
