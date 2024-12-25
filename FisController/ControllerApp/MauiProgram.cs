using ControllerApp.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace ControllerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp(true)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //Check environment variable
            var access_token = Environment.GetEnvironmentVariable("MAPBOX_ACCESS_TOKEN");

            //if (string.IsNullOrEmpty(access_token))
            //{
            //    throw new InvalidOperationException("MAPBOX_ACCESS_TOKEN environment variable is not set.");
            //}
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
            //builder.Services.AddSingleton<IFileSource, FileSource>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
