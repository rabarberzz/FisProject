using Mapbox.Json;
using Mapbox.Platform;
using Mapbox.Utils.JsonConverters;
using System.Text;

namespace ControllerApp.SearchBox.Suggestion
{
    public class Suggestions
    {
        private readonly IFileSource fileSource;

        public Suggestions(IFileSource fileSource)
        {
            this.fileSource = fileSource;
        }

        public IAsyncRequest Query(SearchboxResource resource, Action<SuggestionResponse> callback)
        {
            return fileSource.Request(resource.GetUrl(), delegate (Response response)
            {
                string @string = Encoding.UTF8.GetString(response.Data);
                SuggestionResponse suggestionResponse = Deserialize(@string);
                callback(suggestionResponse);
            });
        }

        internal SuggestionResponse Deserialize(string arr)
        {
            return JsonConvert.DeserializeObject<SuggestionResponse>(arr, JsonConverters.Converters);
        }
    }
}
