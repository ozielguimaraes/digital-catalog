using System;
using MeuCatalogo.Core.Abstractions;
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
using MeuCatalogo.Features.Categoria.Data.Local;
using MeuCatalogo.Features.Categoria.Data.Remote;
using MeuCatalogo.Features.Categoria.Data.Sync;
using MeuCatalogo.Features.Categoria.UseCases;
using MeuCatalogo.Features.Cliente;
using MeuCatalogo.Features.Cliente.Data;
using MeuCatalogo.Features.Cliente.Data.Remote;
using MeuCatalogo.Features.Cliente.UseCases;
using MeuCatalogo.Features.Estoque;
using MeuCatalogo.Features.Financeiro;
using MeuCatalogo.Features.Financeiro.Data;
using MeuCatalogo.Features.Financeiro.Data.Remote;
using MeuCatalogo.Features.Financeiro.UseCases;
using MeuCatalogo.Features.Fornecedor;
using MeuCatalogo.Features.Fornecedor.Data;
using MeuCatalogo.Features.Fornecedor.Data.Remote;
using MeuCatalogo.Features.Fornecedor.UseCases;
using MeuCatalogo.Features.Home;
using MeuCatalogo.Features.Mais;
using MeuCatalogo.Features.Pedido;
using MeuCatalogo.Features.Pedido.Data;
using MeuCatalogo.Features.Pedido.Data.Remote;
using MeuCatalogo.Features.Pedido.UseCases;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Data.Remote;
using MeuCatalogo.Features.Produto.Data.Sync;
using MeuCatalogo.Features.Produto.Presentation;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Core.Abstractions.Imaging;
using MeuCatalogo.Infrastructure;
using MeuCatalogo.Infrastructure.Database;
using MeuCatalogo.Infrastructure.Imaging;
using MeuCatalogo.Infrastructure.SyncEngine;
using System.Net;
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
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (retryAttempt, response, _) =>
                {
                    var retryAfter = response.Result?.Headers?.RetryAfter;
                    if (retryAfter is not null)
                    {
                        if (retryAfter.Delta.HasValue) return retryAfter.Delta.Value;
                        if (retryAfter.Date.HasValue)
                        {
                            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                            if (delay > TimeSpan.Zero) return delay;
                        }
                    }
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                },
                onRetryAsync: (_, _, _, _) => Task.CompletedTask);

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

        builder.Services
            .AddRefitClient<IPedidoApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<IClienteApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<IFornecedorApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<LoggingHttpClientHandler>()
            .AddPolicyHandler(retryPolicy);

        builder.Services
            .AddRefitClient<IFinanceiroApi>()
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
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<AppDbContext>();
        builder.Services.AddSingleton<IImageProcessor, MauiImageProcessor>();
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
        builder.Services.AddTransient<GetCatalogosLocalUseCase>();
        builder.Services.AddTransient<SyncCatalogosUseCase>();
        builder.Services.AddTransient<CreateCatalogoUseCase>();
        builder.Services.AddTransient<DeleteCatalogoUseCase>();
        builder.Services.AddTransient<SetCatalogoEmUsoUseCase>();
        builder.Services.AddTransient<ICategoriaRemoteDataSource, CategoriaRemoteDataSource>();
        builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
        builder.Services.AddTransient<ICategoriaLocalRepository, CategoriaLocalRepository>();
        builder.Services.AddTransient<GetCategoriasByCatalogoUseCase>();
        builder.Services.AddTransient<SyncCategoriasByCatalogoUseCase>();
        builder.Services.AddTransient<CreateCategoriaUseCase>();
        builder.Services.AddTransient<UpdateCategoriaUseCase>();
        builder.Services.AddTransient<IProdutoRemoteDataSource, ProdutoRemoteDataSource>();
        builder.Services.AddTransient<IProdutoRepository, ProdutoRepository>();
        builder.Services.AddTransient<IProdutoLocalRepository, ProdutoLocalRepository>();
        builder.Services.AddTransient<IProdutoImagemLocalRepository, ProdutoImagemLocalRepository>();
        builder.Services.AddTransient<GetProdutosByCatalogoIdUseCase>();
        builder.Services.AddTransient<GetProdutoByIdUseCase>();
        builder.Services.AddTransient<GetProdutoForEditOfflineFirstUseCase>();
        builder.Services.AddTransient<CreateProdutoRemoteUseCase>();
        builder.Services.AddTransient<UpdateProdutoRemoteUseCase>();
        builder.Services.AddTransient<UpsertProdutoOfflineFirstUseCase>();
        builder.Services.AddTransient<DeleteProdutoRemoteUseCase>();
        builder.Services.AddTransient<DeleteProdutoOfflineFirstUseCase>();
        builder.Services.AddTransient<UploadProdutoImageUseCase>();
        builder.Services.AddTransient<GetProdutosUseCase>();
        builder.Services.AddTransient<CreateProdutoUseCase>();
        builder.Services.AddTransient<SyncProdutosByCatalogoUseCase>();
        builder.Services.AddTransient<IPedidoRemoteDataSource, PedidoRemoteDataSource>();
        builder.Services.AddTransient<IPedidoRepository, PedidoRepository>();
        builder.Services.AddTransient<GetPedidosUseCase>();
        builder.Services.AddTransient<DeletePedidoUseCase>();
        builder.Services.AddTransient<CreatePedidoUseCase>();

        builder.Services.AddTransient<IClienteRemoteDataSource, ClienteRemoteDataSource>();
        builder.Services.AddTransient<IClienteRepository, ClienteRepository>();
        builder.Services.AddTransient<GetClientesUseCase>();
        builder.Services.AddTransient<UpsertClienteUseCase>();
        builder.Services.AddTransient<DeleteClienteUseCase>();

        builder.Services.AddTransient<IFornecedorRepository, FornecedorRepository>();
        builder.Services.AddTransient<GetFornecedoresUseCase>();

        builder.Services.AddTransient<IFinanceiroRepository, FinanceiroRepository>();
        builder.Services.AddTransient<IContaRepository, ContaRepository>();
        builder.Services.AddTransient<ICategoriaFinanceiraRepository, CategoriaFinanceiraRepository>();
        builder.Services.AddTransient<IFaturaRepository, FaturaRepository>();
        builder.Services.AddTransient<IRecorrenciaRepository, RecorrenciaRepository>();
        builder.Services.AddTransient<ITransferenciaRepository, TransferenciaRepository>();
        builder.Services.AddTransient<ILancamentoBaixaRepository, LancamentoBaixaRepository>();
        builder.Services.AddTransient<IComprovanteFinanceiroRepository, ComprovanteFinanceiroRepository>();
        builder.Services.AddTransient<IRelatorioFinanceiroRepository, RelatorioFinanceiroRepository>();
        builder.Services.AddTransient<IExtratoRepository, ExtratoRepository>();

        builder.Services.AddTransient<GetFinanceiroResumoUseCase>();
        builder.Services.AddTransient<GetLancamentosUseCase>();
        builder.Services.AddTransient<CriarLancamentoUseCase>();
        builder.Services.AddTransient<AtualizarLancamentoUseCase>();
        builder.Services.AddTransient<RemoverLancamentoUseCase>();
        builder.Services.AddTransient<GetContasUseCase>();
        builder.Services.AddTransient<CriarContaUseCase>();
        builder.Services.AddTransient<AtualizarContaUseCase>();
        builder.Services.AddTransient<RemoverContaUseCase>();
        builder.Services.AddTransient<GetCategoriasFinanceirasUseCase>();
        builder.Services.AddTransient<CriarCategoriaFinanceiraUseCase>();
        builder.Services.AddTransient<AtualizarCategoriaFinanceiraUseCase>();
        builder.Services.AddTransient<RemoverCategoriaFinanceiraUseCase>();
        builder.Services.AddTransient<CriarSubcategoriaFinanceiraUseCase>();
        builder.Services.AddTransient<GetFaturaUseCase>();
        builder.Services.AddTransient<ListarFaturasPorContaUseCase>();
        builder.Services.AddTransient<GetRecorrenciasUseCase>();
        builder.Services.AddTransient<CriarRecorrenciaUseCase>();
        builder.Services.AddTransient<AtualizarRecorrenciaUseCase>();
        builder.Services.AddTransient<RemoverRecorrenciaUseCase>();
        builder.Services.AddTransient<CriarTransferenciaUseCase>();
        builder.Services.AddTransient<GetBaixasUseCase>();
        builder.Services.AddTransient<RegistrarBaixaUseCase>();
        builder.Services.AddTransient<RemoverBaixaUseCase>();
        builder.Services.AddTransient<UploadComprovanteUseCase>();
        builder.Services.AddTransient<GetRelatorioPorCategoriaUseCase>();
        builder.Services.AddTransient<GetExtratoPorContaUseCase>();
        builder.Services.AddTransient<GetExtratoConsolidadoUseCase>();

        builder.Services.AddTransient<ISyncHandler, CatalogoPullSyncHandler>();
        builder.Services.AddTransient<ISyncHandler, CategoriaPullSyncHandler>();
        builder.Services.AddTransient<ISyncHandler, ProdutoPullSyncHandler>();
        builder.Services.AddTransient<ISyncHandler, ProdutoUpsertSyncHandler>();
        builder.Services.AddTransient<ISyncHandler, ProdutoDeleteSyncHandler>();

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

        //Feature Home
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<HomePageViewModel>();

        //Feature Catalogos
        builder.Services.AddTransient<CatalogoListaPage>();
        builder.Services.AddTransient<CatalogoListaPageViewModel>();
        builder.Services.AddTransient<CatalogoAdicionarPage>();
        builder.Services.AddTransient<CatalogoAdicionarPageViewModel>();
        builder.Services.AddTransient<CatalogoDetalhePage>();
        builder.Services.AddTransient<CatalogoDetalhePageViewModel>();
        builder.Services.AddTransient<CatalogoPublicaPage>();
        builder.Services.AddTransient<CatalogoPublicaPageViewModel>();
        builder.Services.AddBottomSheet<CatalogoEmUsoBottomSheet, CatalogoEmUsoBottomSheetViewModel>(BottomSheetKeys.CatalogoEmUso);

        //Feature Produtos
        builder.Services.AddTransient<ProdutoListaPage>();
        builder.Services.AddTransient<ProdutoListaPageViewModel>();
        builder.Services.AddTransient<ProdutoAdicionarPage>();
        builder.Services.AddTransient<ProdutoAdicionarPageViewModel>();
        builder.Services.AddTransient<ProdutoDetalhePage>();
        builder.Services.AddTransient<ProdutoDetalhePageViewModel>();

        //Feature Pedido
        builder.Services.AddTransient<PedidoListaPage>();
        builder.Services.AddTransient<PedidoListaPageViewModel>();
        builder.Services.AddTransient<PedidoNovoPage>();
        builder.Services.AddTransient<PedidoNovoPageViewModel>();
        builder.Services.AddBottomSheet<MeuCatalogo.Features.Pedido.Bottom.PedidoClienteBottomSheet, MeuCatalogo.Features.Pedido.Bottom.PedidoClienteBottomSheetViewModel>(BottomSheetKeys.PedidoCliente);
        builder.Services.AddBottomSheet<MeuCatalogo.Features.Pedido.Bottom.PedidoProdutoBottomSheet, MeuCatalogo.Features.Pedido.Bottom.PedidoProdutoBottomSheetViewModel>(BottomSheetKeys.PedidoProduto);

        //Feature Cliente
        builder.Services.AddTransient<ClienteListaPage>();
        builder.Services.AddTransient<ClienteListaPageViewModel>();
        builder.Services.AddTransient<ClienteUpsertPage>();
        builder.Services.AddTransient<ClienteUpsertPageViewModel>();

        //Feature Fornecedor (mock)
        builder.Services.AddTransient<FornecedorListaPage>();
        builder.Services.AddTransient<FornecedorListaPageViewModel>();

        //Feature Financeiro
        builder.Services.AddTransient<FinanceiroPage>();
        builder.Services.AddTransient<FinanceiroPageViewModel>();
        builder.Services.AddTransient<ContasListaPage>();
        builder.Services.AddTransient<ContasListaPageViewModel>();
        builder.Services.AddTransient<ContaEdicaoPage>();
        builder.Services.AddTransient<ContaEdicaoPageViewModel>();
        builder.Services.AddTransient<CategoriasFinanceirasPage>();
        builder.Services.AddTransient<CategoriasFinanceirasPageViewModel>();
        builder.Services.AddTransient<CategoriaFinanceiraEdicaoPage>();
        builder.Services.AddTransient<CategoriaFinanceiraEdicaoPageViewModel>();
        builder.Services.AddTransient<RecorrenciasPage>();
        builder.Services.AddTransient<RecorrenciasPageViewModel>();
        builder.Services.AddTransient<TransferenciaPage>();
        builder.Services.AddTransient<TransferenciaPageViewModel>();
        builder.Services.AddTransient<BaixaPage>();
        builder.Services.AddTransient<BaixaPageViewModel>();
        builder.Services.AddTransient<FaturaPage>();
        builder.Services.AddTransient<FaturaPageViewModel>();
        builder.Services.AddTransient<RelatoriosPage>();
        builder.Services.AddTransient<RelatoriosPageViewModel>();
        builder.Services.AddTransient<ExtratoPage>();
        builder.Services.AddTransient<ExtratoPageViewModel>();
        builder.Services.AddTransient<ReceberPage>();
        builder.Services.AddTransient<ReceberPageViewModel>();
        builder.Services.AddTransient<PagarPage>();
        builder.Services.AddTransient<PagarPageViewModel>();

        //Feature Mais
        builder.Services.AddTransient<MaisPage>();
        builder.Services.AddTransient<MaisPageViewModel>();

        //Feature Settings

        return builder;
    }
}
