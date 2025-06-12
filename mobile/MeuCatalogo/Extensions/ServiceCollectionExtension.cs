using System;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace MeuCatalogo.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddClientServices(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient()
            .ConfigureHttpClientDefaults(f =>
            {
                {
                    f.AddHttpMessageHandler<LoggingHttpClientHandler>();
                    f.RemoveAllLoggers();
                }
            });

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
        services.AddTransient<LoggingHttpClientHandler>();
        services.AddTransient<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // Feature Auth
        services.AddTransient<LoginPage>();
        services.AddTransient<LoginPageViewModel>();
        services.AddTransient<SignupPage>();
        services.AddTransient<SignupPageViewModel>();

        return services;
    }
}
