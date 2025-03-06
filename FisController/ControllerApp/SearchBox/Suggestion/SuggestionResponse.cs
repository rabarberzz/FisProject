using Mapbox.Json;

namespace ControllerApp.SearchBox.Suggestion
{
    [Serializable]
    public class SuggestionResponse
    {
        [JsonProperty("suggestions")]
        public List<SuggestionObject> Suggestions { get; set; }

        [JsonProperty("attribution")]
        public string Attribution { get; set; }
    }
}
