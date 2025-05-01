using ControllerApp.Services;
using Mapbox.Directions;
using Mapsui;
using Mapsui.Projections;

namespace ControllerApp
{
    public partial class MainPage : ContentPage
    {
        private MapboxService mapboxService;
        private LocationService locationService;
        private MapsuiService mapsuiService;
        private DirectionsResponse? localResponse;
        private NavigationService navigationService;

        public MainPage(MapboxService mapboxSvc, LocationService locationSvc, MapsuiService mapsuiSvc, NavigationService navSvc)
        {
            InitializeComponent();
            mapboxService = mapboxSvc;
            mapboxService.DirectionsResponseReceived += OnDirectionsReceived;
            mapboxService.RequestFailed += OnHttpRequestFailed;

            locationService = locationSvc;
            locationService.LocationUpdatedMapsui += OnLocationUpdated;
            locationService.LocationUpdateFailed += OnExceptionAlert;

            mapsuiService = mapsuiSvc;

            mapControlElement.Content = mapsuiService.MapControl;

            navigationService = navSvc;
        }

        private void OnLocationUpdated(object? sender, MPoint locationPoint)
        {
            if (locationPoint != null && mapsuiService.MapControl.Map != null)
            {
                mapsuiService.LocationLayer.UpdateMyLocation(locationPoint);
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            mapboxService.GetDirections();
        }

        private void OnDirectionsReceived(object? sender, DirectionsResponse response)
        {
            var directions = response;
            if (directions.Code != null && mapsuiService != null)
            {
                responseEntry.Text = directions.Code;
                mapsuiService.SetupPointsOnMap(directions);
                mapsuiService.SetupLineOnMap(directions);
                localResponse = response;
            }
        }

        private void OnHttpRequestFailed(object? sender, Exception e)
        {
            responseEntry.Text = e.Message;
        }

        private void OnExceptionAlert(object? sender, Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() => DisplayAlert("Error", exception.Message, "OK"));
        }

        private void ClosestPoint_Clicked(object sender, EventArgs e)
        {
            var location = locationService.CurrentLocation;
            if (location != null)
            {
                var locationMPoint = new MPoint(location.Longitude, location.Latitude);
                var convertedLocation = Mapsui.Projections.SphericalMercator.FromLonLat(locationMPoint);
                var result = mapsuiService.GetClosestGeometryPointFromCoordinates(convertedLocation);

                var convertedResult = new MPoint();
                if (result != null)
                {
                    convertedResult = SphericalMercator.ToLonLat(new MPoint(result.X, result.Y));
                }
            }
        }

        private void ComparisonButtonClicked(object sender, EventArgs e)
        {
            navigationService.IncrementManeuver();
        }

        private void CalculateToManeuverClicked(object sender, EventArgs e)
        {
            var location = locationService.CurrentLocation;
            if (location != null)
            {
                var locationMPoint = new MPoint(location.Longitude, location.Latitude);
                var convertedLocation = Mapsui.Projections.SphericalMercator.FromLonLat(locationMPoint);

                mapsuiService.CalculateDistanceStraightToPoint(convertedLocation);
            }
        }
    }
}
