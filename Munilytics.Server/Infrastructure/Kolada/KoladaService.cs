using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Models.DTOs;
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of municipalities from the Kolada API.
        /// </summary>
        /// <returns>A list of KoladaMunicipalityDto objects representing the municipalities. Returns an empty list if no
        /// municipalities are found.</returns>
        public async Task<List<KoladaMunicipalityDto>> GetMunicipalitiesAsync<T>(CancellationToken ct = default)
        {
            var response = await GetAsync<KoladaResponseDto<KoladaMunicipalityDto>>("municipality", ct);
            return response?.Values ?? new List<KoladaMunicipalityDto>();
        }

        /// <summary>
        /// Asynchronously retrieves a list of KPIs from the Kolada API.
        /// </summary>
        /// <returns>A list of KoladaKpiDto objects representing the KPIs. Returns an empty list if no
        /// KPIs are found.</returns>
        public async Task<List<KoladaKpiDto>> GetKpisAsync<T>(CancellationToken ct = default)
        {
            var response = await GetAsync<KoladaResponseDto<KoladaKpiDto>>("kpi", ct);
            return response?.Values ?? new List<KoladaKpiDto>();
        }
    }
}
