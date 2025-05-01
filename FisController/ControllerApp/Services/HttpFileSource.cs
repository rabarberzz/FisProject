using Mapbox.Platform;

namespace ControllerApp.Services
{
    public class HttpFileSource : IFileSource
    {
        private readonly HttpClient _httpClient;
        private readonly string? _accessToken;
        private readonly IConfigurationService _configurationService;

        public event EventHandler<Exception>? RequestFailed;

        public HttpFileSource()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(4);
            _configurationService = IPlatformApplication.Current?.Services.GetService<IConfigurationService>() ?? throw new Exception("Configuration service not found");
            _accessToken = _configurationService.GetAccessToken();
        }

        public IAsyncRequest Request(string url, Action<Response> callback, int timeout = 4)
        {
            if (_accessToken != null)
            {
                url += "&access_token=" + _accessToken;
            }
            else
            {
                RequestFailed?.Invoke(this, new Exception("Access token not found"));
            }
            var request = new HttpAsyncRequest(url, callback, _httpClient, timeout);
            request.RequestFailed += OnRequestFailed;
            return request;
        }

        private void OnRequestFailed(object? sender, Exception e)
        {
            RequestFailed?.Invoke(this, e);
        }
    }
}
