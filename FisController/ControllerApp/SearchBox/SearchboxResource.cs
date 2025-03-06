using Mapbox.Platform;

namespace ControllerApp.SearchBox
{
    public class SearchboxResource : Mapbox.Platform.Resource
    {
        private string apiEndpoint = "search/searchbox/v1/";

        private SearchboxEndpoints endpoint;

        private Guid sessionToken;

        public string Query { get; set; }

        public string Language { get; set; }

        public int Limit { get; set; }

        public string Proximity { get; set; }

        public string Bbox { get; set; }

        public string Country { get; set; }

        public string Types { get; set; }

        public string NavigationProfile { get; set; }

        public string Origin { get; set; }

        public string MapboxId { get; set; }

        public override string ApiEndpoint => apiEndpoint;

        public SearchboxResource(SearchboxEndpoints endpoint, Guid sessionToken)
        {
            this.endpoint = endpoint;
            this.sessionToken = sessionToken;
        }

        public override string GetUrl()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Language))
            {
                parameters.Add("language", Language.ToLower());
            }

            if (!string.IsNullOrEmpty(Origin))
            {
                parameters.Add("origin", Origin.ToLower());
            }

            if (sessionToken != Guid.Empty)
            {
                parameters.Add("session_token", sessionToken.ToString());
            }

            if (!string.IsNullOrEmpty(NavigationProfile))
            {
                parameters.Add("navigationProfile", NavigationProfile.ToLower());
            }

            if (endpoint == SearchboxEndpoints.Retrieve)
            {
                if (!string.IsNullOrEmpty(MapboxId))
                {
                    parameters.Add("", MapboxId);
                }

                return "https://api.mapbox.com/" + ApiEndpoint + endpoint.ToString() + Mapbox.Platform.Resource.EncodeQueryString(parameters);
            }

            if (!string.IsNullOrEmpty(Query))
            {
                parameters.Add("q", Query.ToLower());
            }

            if (Limit > 0)
            {
                parameters.Add("limit", Limit.ToString());
            }

            if (!string.IsNullOrEmpty(Proximity))
            {
                parameters.Add("proximity", Proximity.ToLower());
            }

            if (!string.IsNullOrEmpty(Bbox))
            {
                parameters.Add("bbox", Bbox.ToLower());
            }

            if (!string.IsNullOrEmpty(Country))
            {
                parameters.Add("country", Country.ToLower());
            }

            if (!string.IsNullOrEmpty(Types))
            {
                parameters.Add("types", Types.ToLower());
            }

            return "https://api.mapbox.com/" + ApiEndpoint + endpoint.ToString() + Mapbox.Platform.Resource.EncodeQueryString(parameters);
        }
    }
}
