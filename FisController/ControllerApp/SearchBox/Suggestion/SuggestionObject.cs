using Mapbox.Json;

namespace ControllerApp.SearchBox.Suggestion
{
    [Serializable]
    public class SuggestionObject
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

        [JsonProperty("full_address")]
        public string FullAddress { get; set; }

        [JsonProperty("place_formatted")]
        public string PlaceFormatted { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("eta")]
        public double Eta { get; set; }
    }
}
