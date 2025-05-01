using Mapsui;
using Timer = System.Timers.Timer;

namespace ControllerApp.Services
{
    public class LocationService
    {
        private Timer locationUpdateTimer;

        /// <summary>
        /// Event that is triggered when the location is updated.
        /// <value>
        /// Value passed is an MPoint object which is already converted/projected for ready use in mapsui.
        /// </value>
        /// </summary>
        public event EventHandler<MPoint>? LocationUpdatedMapsui;

        /// <summary>
        /// Event that is triggered when the location is updated.
        /// <value>
        /// Value passed is a Location object which is the raw location from Geolocation service.
        /// </value>
        /// </summary>
        public event EventHandler<Location>? LocationUpdated;

        public Location? CurrentLocation { get; private set; }
        public event EventHandler<Exception>? LocationUpdateFailed;

        public LocationService()
        {
            locationUpdateTimer = new Timer(2000);
            locationUpdateTimer.Elapsed += UpdateLocation;
            locationUpdateTimer.Start();
        }

        private void UpdateLocation(object? sender, EventArgs e)
        {
            MainThread.InvokeOnMainThreadAsync(InvokeCurrentLocationAsync);
        }

        private async Task InvokeCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    location = await Geolocation.GetLastKnownLocationAsync();
                }

                if (location != null)
                {
                    var locationMPoint = new MPoint(location.Longitude, location.Latitude);
                    LocationUpdatedMapsui?.Invoke(this, Mapsui.Projections.SphericalMercator.FromLonLat(locationMPoint));
                    LocationUpdated?.Invoke(this, location);
                    CurrentLocation = location;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                LocationUpdateFailed?.Invoke(this, fnsEx);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                LocationUpdateFailed?.Invoke(this, fneEx);
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                LocationUpdateFailed?.Invoke(this, pEx);
            }
            catch (Exception ex)
            {
                // Unable to get location
                LocationUpdateFailed?.Invoke(this, ex);
            }
        }
    }
}
