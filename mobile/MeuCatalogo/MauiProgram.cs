using CommunityToolkit.Maui;
using MeuCatalogo.Extensions;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Plugin.Fingerprint;
using MeuCatalogo.Components;

namespace MeuCatalogo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        EntryHandler.Mapper.AppendToMapping("NoEmoji", (handler, view) =>
        {
#if ANDROID
            // Set input type to avoid emoji keyboard
            handler.PlatformView.InputType = Android.Text.InputTypes.ClassNumber
                                             | Android.Text.InputTypes.NumberFlagDecimal
                                             | Android.Text.InputTypes.NumberFlagSigned;

            // Optional: Disable EmojiCompat if necessary (commented unless you’ve initialized it)
            // AndroidX.Emoji2.Text.EmojiCompat.Config = null;
#endif
        });
        EntryHandler.Mapper.AppendToMapping("NoEmoji", (handler, view) =>
        {
#if ANDROID
    if (view is CustomEntry)
    {
        handler.PlatformView.InputType = Android.Text.InputTypes.ClassNumber
                                         | Android.Text.InputTypes.NumberFlagDecimal
                                         | Android.Text.InputTypes.NumberFlagSigned;
    }
#endif
        });

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            //.ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Lato-Light.ttf", "LatoLight");
                fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
                fonts.AddFont("Lato-Bold.ttf", "LatoBold");
            });

#if DEBUG
        // builder.Logging.AddDebug();
        // builder.Services.AddLogging(configure => configure.AddDebug());
#endif
        builder.Logging.AddConsole();
        builder.Services.AddClientServices(ApiConstants.BaseUrl)
            .AddApplicationServices()
            .AddViewModels();
        builder.Services.AddSingleton<IAppInfo>(AppInfo.Current);

        builder.Services.AddSingleton(CrossFingerprint.Current);

        return builder.Build();
    }
}
