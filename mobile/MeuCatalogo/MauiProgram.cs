using CommunityToolkit.Maui;
using MeuCatalogo.Extensions;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddClientServices("http://catalogo-api.sanyz.com.br/api")
            .AddApplicationServices()
            .AddViewModelServices();

        return builder.Build();
    }
}
