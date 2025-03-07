using Mapbox.Json;

namespace ControllerApp.SearchBox.Retrieve
{
    [Serializable]
    public class RetrieveFeature
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("geometry")]
        public RetrieveGeometry Geometry { get; set; }

        [JsonProperty("properties")]
        public RetrieveProperty Properties { get; set; }
    }
}
