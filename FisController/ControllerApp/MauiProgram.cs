﻿using ControllerApp.Services;
using ControllerApp.ViewModels;
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

            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
            builder.Services.AddSingleton<HttpFileSource>();
            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddSingleton<MapboxService>();
            builder.Services.AddTransient<BluetoothPage>();
            builder.Services.AddSingleton<BleService>();
            builder.Services.AddSingleton<FisNavigationService>();
            builder.Services.AddSingleton<MapsuiService>();
            builder.Services.AddSingleton<NavigationService>();
            builder.Services.AddSingleton<DevicesViewModel>(provider =>
            {
                var bleService = provider.GetRequiredService<BleService>();
                return new DevicesViewModel(bleService);
            });
            builder.Services.AddSingleton<EspConfigService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
