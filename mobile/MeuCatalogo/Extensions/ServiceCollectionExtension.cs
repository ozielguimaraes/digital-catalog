using System;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Catalogo.ApiClients;
using MeuCatalogo.Features.Categoria;
using MeuCatalogo.Features.Categoria.ApiClients;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Produto.ApiClients;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Plugin.Maui.BottomSheet.Hosting;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace MeuCatalogo.Extensions;

public static class ServiceCollectionExtension
{
    public static MauiAppBuilder AddClientServices(this MauiAppBuilder builder, string baseUrl)
    {
        builder.Services.AddHttpClient()
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
        builder.Services
            .AddRefitClient<IAuthApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
            })
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<ICatalogoApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<ICategoriaApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<IProdutoApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        return builder;
    }

    public static MauiAppBuilder AddApplicationServices(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<LoggingHttpClientHandler>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddTransient<IAuthService, AuthService>();
        builder.Services.AddTransient<ICatalogoService, CatalogoService>();
        builder.Services.AddTransient<ICategoriaService, CategoriaService>();
        builder.Services.AddTransient<IProdutoService, ProdutoService>();

        return builder;
    }

    public static MauiAppBuilder AddViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<AppShellViewModel>();

        // Feature Auth
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<SignupPage>();
        builder.Services.AddTransient<SignupPageViewModel>();

        //Feature Categoria
        builder.Services.AddTransient<CategoriaBottomSheet>();
        builder.Services.AddBottomSheet<CategoriaBottomSheet, CategoriaBottomSheetViewModel>(BottomSheetKeys.ListaCategoria);

        //Feature Catalogos
        builder.Services.AddTransient<CatalogoListaPage>();
        builder.Services.AddTransient<CatalogoListaPageViewModel>();
        builder.Services.AddTransient<CatalogoAdicionarPage>();
        builder.Services.AddTransient<CatalogoAdicionarPageViewModel>();

        //Feature Produtos
        builder.Services.AddTransient<ProdutoListaPage>();
        builder.Services.AddTransient<ProdutoListaPageViewModel>();
        builder.Services.AddTransient<ProdutoAdicionarPage>();
        builder.Services.AddTransient<ProdutoAdicionarPageViewModel>();

        //Feature Settings

        return builder;
    }
}
