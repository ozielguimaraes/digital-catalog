using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Produto.ApiClients;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
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

        services
            .AddRefitClient<ICatalogoApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        services
            .AddRefitClient<IProdutoApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<LoggingHttpClientHandler>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<ICatalogoService, CatalogoService>();
        services.AddTransient<IProdutoService, ProdutoService>();

        return services;
    }

    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<AppShellViewModel>();

        // Feature Auth
        services.AddTransient<LoginPage>();
        services.AddTransient<LoginPageViewModel>();
        services.AddTransient<SignupPage>();
        services.AddTransient<SignupPageViewModel>();

        //Feature Catalogos
        services.AddTransient<CatalogoListaPage>();
        services.AddTransient<CatalogoListaPageViewModel>();
        services.AddTransient<CatalogoAdicionarPage>();
        services.AddTransient<CatalogoAdicionarPageViewModel>();

        //Feature Produtos
        services.AddTransient<ProdutoListaPage>();
        services.AddTransient<ProdutoListaPageViewModel>();
        services.AddTransient<ProdutoAdicionarPage>();
        services.AddTransient<ProdutoAdicionarPageViewModel>();

        //Feature Settings

        return services;
    }
}
