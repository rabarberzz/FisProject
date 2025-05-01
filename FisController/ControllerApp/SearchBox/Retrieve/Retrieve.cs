using Mapbox.Json;
using Mapbox.Platform;
using Mapbox.Utils.JsonConverters;
using System.Text;

namespace ControllerApp.SearchBox.Retrieve
{
    public class Retrieve
    {
        private readonly IFileSource fileSource;

        public Retrieve(IFileSource fileSource)
        {
            this.fileSource = fileSource;
        }

        public IAsyncRequest Query(SearchboxResource resource, Action<RetrieveResponse> callback)
        {
            return fileSource.Request(resource.GetUrl(), delegate (Response response)
            {
                string @string = Encoding.UTF8.GetString(response.Data);
                RetrieveResponse retrieveResponse = Deserialize(@string);
                callback(retrieveResponse);
            });
        }

        internal RetrieveResponse Deserialize(string arr)
        {
            return JsonConvert.DeserializeObject<RetrieveResponse>(arr, JsonConverters.Converters);
        }
    }
}
