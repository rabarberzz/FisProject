using Mapsui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace ControllerApp.Services
{
    public class NavigationService
    {
        private Timer locationUpdateTimer;
        public event EventHandler<MPoint>? LocationUpdated;
        public MPoint? CurrentLocation { get; private set; }

        public NavigationService()
        {
            locationUpdateTimer = new Timer(2000);
            locationUpdateTimer.Elapsed += UpdateLocation;
            locationUpdateTimer.Start();
        }

        private void UpdateLocation(object? sender, EventArgs e)
        {
            _ = InvokeCurrentLocationAsync();
        }

        private async Task InvokeCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    var locationMPoint = new MPoint(location.Longitude, location.Latitude);
                    LocationUpdated?.Invoke(this, Mapsui.Projections.SphericalMercator.FromLonLat(locationMPoint));
                    CurrentLocation = Mapsui.Projections.SphericalMercator.FromLonLat(locationMPoint);
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
        }
    }
}
