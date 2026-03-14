using System;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Auth.Data.Local;
using MeuCatalogo.Features.Auth.Data.Remote;
using MeuCatalogo.Features.Auth.UseCases;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Catalogo.Data;
using MeuCatalogo.Features.Catalogo.Data.Local;
using MeuCatalogo.Features.Catalogo.Data.Remote;
using MeuCatalogo.Features.Catalogo.Data.Sync;
using MeuCatalogo.Features.Catalogo.UseCases;
using MeuCatalogo.Features.Categoria;
using MeuCatalogo.Features.Categoria.Data;
using MeuCatalogo.Features.Categoria.Data.Remote;
using MeuCatalogo.Features.Categoria.UseCases;
using MeuCatalogo.Features.Estoque;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Data.Remote;
using MeuCatalogo.Features.Produto.Data.Sync;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using MeuCatalogo.Infrastructure.Database;
using MeuCatalogo.Infrastructure.SyncEngine;
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
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<ICatalogoApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<ICategoriaApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<IProdutoApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        return builder;
    }

    public static MauiAppBuilder AddApplicationServices(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<LoggingHttpClientHandler>();
        builder.Services.AddTransient<AuthenticationHandler>();
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<AppDbContext>();
        builder.Services.AddSingleton<ISyncEngine, SyncEngineService>();
        builder.Services.AddSingleton<IAuthLocalDataSource, AuthLocalDataSource>();
        builder.Services.AddTransient<IAuthRemoteDataSource, AuthRemoteDataSource>();
        builder.Services.AddTransient<IAuthRepository, AuthRepository>();
        builder.Services.AddTransient<SigninUseCase>();
        builder.Services.AddTransient<SignupUseCase>();
        builder.Services.AddTransient<GetCurrentUserUseCase>();
        builder.Services.AddTransient<GetStartupRouteUseCase>();
        builder.Services.AddTransient<LogoutUseCase>();
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<ICatalogoRemoteDataSource, CatalogoRemoteDataSource>();
        builder.Services.AddTransient<ICatalogoRepository, CatalogoRepository>();
        builder.Services.AddTransient<ICatalogoLocalRepository, CatalogoLocalRepository>();
        builder.Services.AddTransient<GetCatalogosUseCase>();
        builder.Services.AddTransient<CreateCatalogoUseCase>();
        builder.Services.AddTransient<DeleteCatalogoUseCase>();
        builder.Services.AddTransient<SetCatalogoFavoritoUseCase>();
        builder.Services.AddTransient<ICategoriaRemoteDataSource, CategoriaRemoteDataSource>();
        builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
        builder.Services.AddTransient<GetCategoriasByCatalogoUseCase>();
        builder.Services.AddTransient<CreateCategoriaUseCase>();
        builder.Services.AddTransient<UpdateCategoriaUseCase>();
        builder.Services.AddTransient<IProdutoRemoteDataSource, ProdutoRemoteDataSource>();
        builder.Services.AddTransient<IProdutoRepository, ProdutoRepository>();
        builder.Services.AddTransient<IProdutoLocalRepository, ProdutoLocalRepository>();
        builder.Services.AddTransient<IProdutoImagemLocalRepository, ProdutoImagemLocalRepository>();
        builder.Services.AddTransient<GetProdutosByCatalogoIdUseCase>();
        builder.Services.AddTransient<GetProdutoByIdUseCase>();
        builder.Services.AddTransient<CreateProdutoRemoteUseCase>();
        builder.Services.AddTransient<UpdateProdutoRemoteUseCase>();
        builder.Services.AddTransient<UpsertProdutoOfflineFirstUseCase>();
        builder.Services.AddTransient<DeleteProdutoRemoteUseCase>();
        builder.Services.AddTransient<UploadProdutoImageUseCase>();
        builder.Services.AddTransient<GetProdutosUseCase>();
        builder.Services.AddTransient<CreateProdutoUseCase>();
        builder.Services.AddTransient<SyncProdutosByCatalogoUseCase>();
        builder.Services.AddTransient<ISyncHandler, CatalogoPullSyncHandler>();
        builder.Services.AddTransient<ISyncHandler, ProdutoPullSyncHandler>();
        builder.Services.AddTransient<ISyncHandler, ProdutoUpsertSyncHandler>();
        builder.Services.AddTransient<SyncAfterLoginUseCase>();

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
        builder.Services.AddBottomSheet<CategoriaBottomSheet, CategoriaBottomSheetViewModel>(BottomSheetKeys.ListaCategoria);

        //Feature Categoria
        builder.Services.AddBottomSheet<EstoqueBottomSheet, EstoqueBottomSheetViewModel>(BottomSheetKeys.Estoque);

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
