using System.Net;

namespace ControllerApp.Models
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }

        public HttpResponse(HttpStatusCode statusCode, string content)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }
}
