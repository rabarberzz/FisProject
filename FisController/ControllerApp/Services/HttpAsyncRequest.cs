using Mapbox.Platform;

namespace ControllerApp.Services
{
    public class HttpAsyncRequest : IAsyncRequest
    {
        private readonly HttpClient _httpClient;
        private readonly Action<Response> _callback;
        private readonly HttpRequestMessage _requestMessage;
        private bool _isCompleted;

        public event EventHandler<Exception>? RequestFailed;

        public bool IsCompleted => _isCompleted;

        public HttpAsyncRequest(string url, Action<Response> callback, HttpClient httpClient, int timeout)
        {
            _httpClient = httpClient;
            _callback = callback;
            _requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);

            SendRequestAsync();
        }

        private async void SendRequestAsync()
        {
            try
            {
                var responseMessage = await _httpClient.SendAsync(_requestMessage);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    RequestFailed?.Invoke(this, new Exception($"Request failed with status code {responseMessage.StatusCode}"));
                }

                var responseBody = await responseMessage.Content.ReadAsStringAsync();
                var webRequest = await _httpClient.GetAsync(_requestMessage.RequestUri);
                var response = Response.FromHttpResponseMessage(this, webRequest, null);
                _callback(response);
            }
            catch (Exception ex)
            {
                var response = Response.FromHttpResponseMessage(this, null, ex);
                _callback(response);
                RequestFailed?.Invoke(this, ex);
            }
            finally
            {
                _isCompleted = true;
            }
        }

        public void Cancel()
        {
            _requestMessage?.Dispose();
        }
    }
}
