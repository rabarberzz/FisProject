using ControllerApp.Services;

namespace ControllerApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

#if WINDOWS
            var access_token = Environment.GetEnvironmentVariable("MAPBOX_ACCESS_TOKEN");
            if (!string.IsNullOrEmpty(access_token))
            {
                var configService = IPlatformApplication.Current?.Services.GetService<IConfigurationService>();
                configService?.SetAccessToken(access_token);
            }
            RequestLocationPermission();
#endif
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void RequestLocationPermission()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
            });
        }
    }
}