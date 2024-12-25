namespace ControllerApp.Services
{
    public interface IConfigurationService
    {
        string? GetAccessToken();
        void SetAccessToken(string accessToken);
    }
    public class ConfigurationService : IConfigurationService
    {
        private string? _accessToken;
        public string? GetAccessToken()
        {
            return _accessToken;
        }

        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
        }
    }
}
