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
using Sentry;

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
            .UseSentry(options =>
            {
                options.Dsn = "https://d28b29c06ae140171a04078f72b872b8@o4507538405916672.ingest.us.sentry.io/4510213135925248";

                options.TracesSampleRate = 0.2;
                options.ProfilesSampleRate = 0.1;
                options.AttachStacktrace = true;
                options.MaxBreadcrumbs = 50;
                options.SendDefaultPii = true;
                options.MinimumEventLevel = LogLevel.Warning;
                options.SetBeforeSend((sentryEvent) => sentryEvent.Level < SentryLevel.Warning ? null : sentryEvent);

#if DEBUG
                options.Environment = "debug";
#else
                options.Environment = "production";
#endif
            })
            //.ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Lato-Light.ttf", "LatoLight");
                fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
                fonts.AddFont("Lato-Bold.ttf", "LatoBold");
                fonts.AddFont("Lato-Black.ttf", "LatoBlack");
                fonts.AddFont("Lato-Italic.ttf", "LatoItalic");
                fonts.AddFont("Lora-Regular.ttf", "LoraRegular");
                fonts.AddFont("Lora-Medium.ttf", "LoraMedium");
                fonts.AddFont("Lora-SemiBold.ttf", "LoraSemiBold");
                fonts.AddFont("Lora-Bold.ttf", "LoraBold");
                fonts.AddFont("Lora-Italic.ttf", "LoraItalic");
                fonts.AddFont("Lora-MediumItalic.ttf", "LoraMediumItalic");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentUI");
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
