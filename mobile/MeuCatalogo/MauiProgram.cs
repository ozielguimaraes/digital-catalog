using CommunityToolkit.Maui;
using MeuCatalogo.Extensions;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Plugin.Fingerprint;
using MeuCatalogo.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Plugin.Maui.BottomSheet.Hosting;

namespace MeuCatalogo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        EntryHandler.Mapper.AppendToMapping("NoEmoji", (handler, view) =>
        {
#if ANDROID
            if (view is MeuCatalogo.Components.CustomEntry)
            {
                handler.PlatformView.InputType =
                    Android.Text.InputTypes.ClassNumber
                    | Android.Text.InputTypes.NumberFlagDecimal
                    | Android.Text.InputTypes.NumberFlagSigned;

                // Optional: Disable EmojiCompat if needed
                // AndroidX.Emoji2.Text.EmojiCompat.Config = null;
            }
#endif
        });

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBottomSheet(config => config.CopyPagePropertiesToBottomSheet = true)
            //.ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Lato-Light.ttf", "LatoLight");
                fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
                fonts.AddFont("Lato-Bold.ttf", "LatoBold");
            })
            .AddClientServices(ApiConstants.BaseUrl)
            .AddApplicationServices()
            .AddViewModels();
#if DEBUG
        builder.Logging.AddDebug();
        // builder.Services.AddLogging(configure => configure.AddDebug());
        //builder.Logging.AddConsole();
#endif
        builder.Services.AddSingleton<IAppInfo>(AppInfo.Current);

        builder.Services.AddSingleton(CrossFingerprint.Current);

        return builder.Build();
    }
}
