using Example.ViewModels;
using Microsoft.Extensions.Logging;
using MPowerKit.Lottie;

namespace Example;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMPowerKitLottie()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Cookie-Regular.ttf", "CookieRegular");
                fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FontAwesomeRegular");
                fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FontAwesomeSolid");
            });
        builder.Services.AddTransient<CarsViewModel>();
        builder.Services.AddTransient<ShopViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
