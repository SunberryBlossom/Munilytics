namespace Munilytics.Server.Features.KoladaService
{
    public class KoladaClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.kolada.se/v2/";

        public KoladaClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
