using Android.App;
using Android.Content.PM;
using Android.OS;
using ControllerApp.Services;

namespace ControllerApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var accessToken = Resources.GetString(Resource.String.mapbox_access_token);
            if (accessToken != null)
            {
                var configService = IPlatformApplication.Current?.Services.GetService<IConfigurationService>();
                configService?.SetAccessToken(accessToken);
            }
        }
    }
}
