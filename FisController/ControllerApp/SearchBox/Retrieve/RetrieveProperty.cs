using Mapbox.Json;

namespace ControllerApp.SearchBox.Retrieve
{
    [Serializable]
    public class RetrieveProperty
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_preferred")]
        public string NamePreferred { get; set; }

        [JsonProperty("mapbox_id")]
        public string MapboxId { get; set; }

        [JsonProperty("feature_type")]
        public string FeatureType { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
