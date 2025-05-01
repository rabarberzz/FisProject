using Mapbox.Json;

namespace ControllerApp.SearchBox.Retrieve
{
    [Serializable]
    public class RetrieveResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("features")]
        public List<RetrieveFeature> Features { get; set; }

        [JsonProperty("attribution")]
        public string Attribution { get; set; }
    }
}
