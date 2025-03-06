namespace ControllerApp.SearchBox
{
    public class SearchboxEndpoints
    {
        public static readonly SearchboxEndpoints Suggest = new SearchboxEndpoints("suggest");
        public static readonly SearchboxEndpoints Retrieve = new SearchboxEndpoints("retrieve");

        private readonly string endpoint;

        private SearchboxEndpoints(string endpoint)
        {
            this.endpoint = endpoint;
        }

        public override string ToString()
        {
            return endpoint;
        }
    }
}
