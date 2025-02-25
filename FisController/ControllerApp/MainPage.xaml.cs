using Mapsui;
using Mapsui.Layers;
using Timer = System.Timers.Timer;

namespace ControllerApp
{
    public partial class MainPage : ContentPage
    {
        //int count = 0;
        private MyLocationLayer locationLayer;
        private Timer locationUpdateTimer;

        public MainPage()
        {
            InitializeComponent();
            InitializeMap();
        }

        private async void InitializeMap()
        {
            var mapControl = new Mapsui.UI.Maui.MapControl();

            mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            var currentLocation = await GetCurrentLocationAsync();
            if (currentLocation != null && mapControl.Map != null)
            {
                locationLayer = new MyLocationLayer(mapControl.Map, currentLocation);
                mapControl.Map.Layers.Add(locationLayer);

                StartLocationUpdates();
            }

            Content = mapControl;
        }

        private async Task<MPoint> GetCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
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

        //private MyLocationLayer CreateLocationMarker(double latitude, double longitude)
        //{
        //    var point = new PointFeature(latitude, longitude);
        //    var style = new SymbolStyle
        //    {
        //        SymbolScale = 0.8,
        //        Fill = new Brush(Color.Red),
        //        Outline = new Pen(Color.Black, 2)
        //    };
        //    feature.Styles.Add(style);
        //    return feature;
        //}

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}
    }

}
