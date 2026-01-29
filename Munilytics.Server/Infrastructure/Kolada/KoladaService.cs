using Munilytics.Server.Interfaces;
using System.Text.Json;

namespace Munilytics.Server.Infrastructure.Kolada
{
    public class KoladaService : IKoladaService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.kolada.se/v2/";

        public KoladaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves data from the Kolada API and deserializes the JSON response into the specified type T.
        /// </summary>
        /// <typeparam name="T">The type into which the JSON response is deserialized.</typeparam>
        /// <param name="endpoint">The relative URL of the endpoint to retrieve data from. This value should not include the base URL.</param>
        /// <returns>An instance of type T with the deserialized data from the endpoint.</returns>
        public async Task<T> GetAsync<T>(string endpoint, CancellationToken ct)
        {
            var json = await _httpClient.GetStringAsync(BaseUrl + endpoint, ct);
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }
}
