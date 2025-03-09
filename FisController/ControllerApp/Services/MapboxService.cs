using ControllerApp.SearchBox;
using ControllerApp.SearchBox.Retrieve;
using ControllerApp.SearchBox.Suggestion;
using Mapbox.Directions;
using Mapbox.Utils;
using System.Globalization;

namespace ControllerApp.Services
{
    public class MapboxService
    {

        public event EventHandler<DirectionsResponse>? DirectionsResponseReceived;
        public event EventHandler<Exception>? RequestFailed;
        public event EventHandler<SuggestionResponse>? SuggestionResponseReceived;
        public event EventHandler<RetrieveResponse>? RetrieveResponseReceived;
        private Guid? searchboxSessionToken;
        private HttpFileSource fileSource;

        public MapboxService(HttpFileSource httpFileSource)
        {
            fileSource = httpFileSource;
            fileSource.RequestFailed += OnRequestFailed;
            searchboxSessionToken = Guid.NewGuid();
        }

        public void GetDirections()
        {
            Directions directions = new Directions(fileSource);

            var origin = new Vector2d(56.204758837605596, 25.30941263095276);
            var destination = new Vector2d(56.50672764632251, 25.863789713197978);
            DirectionResource directionResource = new DirectionResource([origin, destination], RoutingProfile.Driving);
            directionResource.Steps = true;
            directionResource.Overview = Overview.Full;
            directionResource.Annotations = ["distance"];

            directions.Query(directionResource, OnDirectionsResponseReceived);
        }

        public void GetDirections(Vector2d locOrigin, Vector2d locDestination)
        {
            Directions directions = new Directions(fileSource);
            DirectionResource directionResource = new DirectionResource([locOrigin, locDestination], RoutingProfile.Driving);
            directionResource.Steps = true;
            directionResource.Overview = Overview.Full;
            directionResource.Annotations = ["distance"];

            directions.Query(directionResource, OnDirectionsResponseReceived);
        }

        public void GetLocationSuggestion()
        {
            if (searchboxSessionToken == null)
            {
                searchboxSessionToken = Guid.NewGuid();
            }

            Suggestions suggestions = new Suggestions(fileSource);
            var query = "Riga, Latvia";
            SearchboxResource searchboxResource = new SearchboxResource(SearchboxEndpoints.Suggest, searchboxSessionToken.Value);
            searchboxResource.Query = query;

            var suggestionResponse = new SuggestionResponse();

            suggestions.Query(searchboxResource, OnSuggestionsResponseReceived);
        }

        public void GetLocationSuggestion(string searchQuery, Location? locationPoint = null)
        {
            if (searchboxSessionToken == null)
            {
                searchboxSessionToken = Guid.NewGuid();
            }

            Suggestions suggestions = new Suggestions(fileSource);
            SearchboxResource searchboxResource = new SearchboxResource(SearchboxEndpoints.Suggest, searchboxSessionToken.Value);
            searchboxResource.Query = searchQuery;
            if (locationPoint != null)
            {
                searchboxResource.Proximity = $"{locationPoint.Longitude.ToString(CultureInfo.InvariantCulture)},{locationPoint.Latitude.ToString(CultureInfo.InvariantCulture)}";
                
                searchboxResource.NavigationProfile = "driving";
            }

            suggestions.Query(searchboxResource, OnSuggestionsResponseReceived);
        }

        public void GetSuggestionRetrieve(SuggestionObject suggestion)
        {
            if (searchboxSessionToken != null)
            {
                Retrieve retrieve = new Retrieve(fileSource);
                SearchboxResource searchboxResource = new SearchboxResource(SearchboxEndpoints.Retrieve, searchboxSessionToken.Value);
                searchboxResource.MapboxId = suggestion.MapboxId;

                retrieve.Query(searchboxResource, OnRetrieveResponseReceived);
            }
        }

        private void OnRetrieveResponseReceived(RetrieveResponse e)
        {
            if (e != null)
            {
                RetrieveResponseReceived?.Invoke(this, e);
            }
        }

        private void OnSuggestionsResponseReceived(SuggestionResponse e)
        {
            if (e != null)
            {
                SuggestionResponseReceived?.Invoke(this, e);
            }
        }

        private void OnDirectionsResponseReceived(DirectionsResponse e)
        {
            if (e != null)
            {
                DirectionsResponseReceived?.Invoke(this, e);
            }
        }

        private void OnRequestFailed(object? sender, Exception e)
        {
            RequestFailed?.Invoke(this, e);
        }
    }
}
