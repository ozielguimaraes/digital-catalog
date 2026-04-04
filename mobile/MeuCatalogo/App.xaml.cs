using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using MeuCatalogo.Infrastructure.Database;
using MeuCatalogo.Infrastructure.SyncEngine;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo;

public partial class App
{
    private readonly IAuthRepository _authRepository;
    private readonly INavigationService _navigationService;
    private readonly IConnectivity _connectivity;
    private readonly ISyncEngine _syncEngine;
    private readonly ILogger<App> _logger;
    private bool _syncLifecycleInitialized;

    public App(
        IAuthRepository authRepository,
        INavigationService navigationService,
        IConnectivity connectivity,
        ISyncEngine syncEngine,
        ILogger<App> logger)
    {
        _authRepository = authRepository;
        _navigationService = navigationService;
        _connectivity = connectivity;
        _syncEngine = syncEngine;
        _logger = logger;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (_authRepository.IsAuthenticated())
        {
            var appShellViewModel = IPlatformApplication.Current!.Services.GetService<AppShellViewModel>()
                                    ?? throw new InvalidOperationException("AppShellViewModel is not registered in the DI container.");

            var shell = new AppShell(appShellViewModel);

            return new Window(shell);
        }

        var loginPageViewModel = IPlatformApplication.Current!.Services.GetService<LoginPageViewModel>()
                                 ?? throw new InvalidOperationException("LoginPageViewModel is not registered in the DI container.");

        return new Window(new LoginPage(loginPageViewModel));
    }

    protected override async void OnHandlerChanged()
    {
        try
        {
            base.OnHandlerChanged();

            if (Handler is not null)
            {
                var db = IPlatformApplication.Current?.Services.GetService<AppDbContext>();
                if (db != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await db.InitializeAsync();
                        }
                        catch
                        {
                        }
                    });
                }

                InitializeSyncLifecycle();
                _ = ProcessPendingSyncAsync();

                // await _navigationService.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    private void InitializeSyncLifecycle()
    {
        if (_syncLifecycleInitialized)
            return;

        _connectivity.ConnectivityChanged += OnConnectivityChanged;
        _syncLifecycleInitialized = true;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
            return;

        _ = ProcessPendingSyncAsync();
    }

    private async Task ProcessPendingSyncAsync()
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            return;

        try
        {
            await _syncEngine.ProcessQueueAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar fila de sincronização.");
        }
    }
}
