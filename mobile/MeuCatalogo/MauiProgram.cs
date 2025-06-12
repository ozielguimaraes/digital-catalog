using CommunityToolkit.Maui;
using MeuCatalogo.Extensions;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Plugin.Fingerprint;

namespace MeuCatalogo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            //.ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // builder.Logging.AddDebug();
        // builder.Services.AddLogging(configure => configure.AddDebug());
#endif
        builder.Logging.AddConsole();
        builder.Services.AddClientServices(ApiConstants.BaseUrl)
            .AddApplicationServices()
            .AddViewModels();

        builder.Services.AddSingleton(CrossFingerprint.Current);

        return builder.Build();
    }
}
