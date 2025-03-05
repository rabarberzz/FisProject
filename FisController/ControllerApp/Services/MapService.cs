using Mapbox.Directions;
using Mapbox.Utils;

namespace ControllerApp.Services
{
    public class MapService
    {

        public event EventHandler<DirectionsResponse>? DirectionsResponseReceived;
        public event EventHandler<Exception>? RequestFailed;

        public void GetDirections()
        {
            // TODO : Implement custom IFileSource, this one isnt supported on this platform
            var fileSource = new HttpFileSource();
            fileSource.RequestFailed += OnRequestFailed;
            Directions directions = new Directions(fileSource);

            var origin = new Vector2d(56.204758837605596, 25.30941263095276);
            var destination = new Vector2d(56.50672764632251, 25.863789713197978);
            DirectionResource directionResource = new DirectionResource([origin, destination], RoutingProfile.Driving);
            directionResource.Steps = true;
            directionResource.Overview = Overview.Full;

            var directionsResponse = new DirectionsResponse();

            directions.Query(directionResource, (response) =>
            {
                if (response != null)
                {
                    DirectionsResponseReceived?.Invoke(this, response);
                }
            });
        }

        private void OnRequestFailed(object? sender, Exception e)
        {
            RequestFailed?.Invoke(this, e);
        }
    }
}
