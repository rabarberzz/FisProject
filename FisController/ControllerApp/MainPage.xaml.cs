using ControllerApp.Services;
using Mapbox.Directions;
using Mapsui;
using Mapsui.Layers;
using Mapsui.UI.Maui;
using Timer = System.Timers.Timer;

namespace ControllerApp
{
    public partial class MainPage : ContentPage
    {
        private MyLocationLayer locationLayer;
        private Timer locationUpdateTimer;
        private MapControl mapControl;
        private MapService mapService;

        public MainPage()
        {
            InitializeComponent();
            InitializeMap();
            mapService = new MapService();
            mapService.DirectionsResponseReceived += OnDirectionsReceived;
            mapService.RequestFailed += OnHttpRequestFailed;

#if WINDOWS
            var access_token = Environment.GetEnvironmentVariable("MAPBOX_ACCESS_TOKEN");
            if (!string.IsNullOrEmpty(access_token))
            {
                var configService = IPlatformApplication.Current?.Services.GetService<IConfigurationService>();
                configService?.SetAccessToken(access_token);
            }
#endif
        }

        private async void InitializeMap()
        {
            mapControl = new MapControl();
            mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            var currentLocation = await GetCurrentLocationAsync();
            if (currentLocation != null && mapControl.Map != null)
            {
                locationLayer = new MyLocationLayer(mapControl.Map, currentLocation);
                mapControl.Map.Layers.Add(locationLayer);

                StartLocationUpdates();
            }

            mapControlElement.Content = mapControl; // Assign mapControl to mapControlElement
        }

        private async Task<MPoint> GetCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    var locationMPoint = new MPoint(location.Longitude, location.Latitude);
                    return Mapsui.Projections.SphericalMercator.FromLonLat(locationMPoint);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }

            return null;
        }

        private async Task UpdateLocation()
        {
            var currentLocation = await GetCurrentLocationAsync();
            if (currentLocation != null && mapControl.Map != null)
            {
                locationLayer.UpdateMyLocation(currentLocation);
            }
        }

        private void StartLocationUpdates()
        {
            locationUpdateTimer = new Timer(2000);
            locationUpdateTimer.Elapsed += async (sender, e) => await UpdateLocation();
            locationUpdateTimer.Start();
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            mapService.GetDirections();
        }

        private void OnDirectionsReceived(object? sender, DirectionsResponse response)
        {
            var directions = response;
            if (directions.Code != null)
            {
                responseEntry.Text = directions.Code;
            }
        }

        private void OnHttpRequestFailed(object? sender, Exception e)
        {
            responseEntry.Text = e.Message;
        }
    }
}
