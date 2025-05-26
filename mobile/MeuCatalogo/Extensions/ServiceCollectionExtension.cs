using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace MeuCatalogo.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddClientServices(this IServiceCollection services, string baseUrl)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Register Refit with Polly and IHttpClientFactory
        services
            .AddRefitClient<IAuthApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
            })
            .AddPolicyHandler(retryPolicy);

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddViewModelServices(this IServiceCollection services)
    {
        services.AddTransient<LoginPage>();
        services.AddTransient<LoginPageViewModel>();

        return services;
    }
}
