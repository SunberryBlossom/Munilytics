using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Interfaces;
using System.Text.Json;

namespace Munilytics.Server.Infrastructure.Kolada
{
    public class KoladaService : IKoladaService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.kolada.se/v3/";

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
        public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct)
        {
            try
            {
                using var response = await _httpClient.GetAsync(BaseUrl + endpoint, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode(); //Throws HttpRequestException if Statuscode is NOT 200-299.

                using var contentStream = await response.Content.ReadAsStreamAsync(ct);
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };

                return await JsonSerializer.DeserializeAsync<T>(contentStream, options, ct);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Netword error while contacting Kolada: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON-error: Could not understand response: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occured: {ex.Message}");
                throw;
            }
        }
    }
}
