using Mapbox.Json;

namespace ControllerApp.SearchBox.Retrieve
{
    [Serializable]
    public class RetrieveGeometry
    {
        [JsonProperty("coordinates")]
        public double[] Coordinates { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
