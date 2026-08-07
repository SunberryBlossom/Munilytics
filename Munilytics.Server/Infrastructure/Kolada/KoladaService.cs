using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Infrastructure.Kolada.DTOs;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Models.DTOs;
using System.Text.Json;

namespace Munilytics.Server.Infrastructure.Kolada
{
    public class KoladaService : IKoladaService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.kolada.se/v3/";
        private ILogger<KoladaService> _logger;

        public KoladaService(HttpClient httpClient, ILogger<KoladaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

        }

        /// <summary>
        /// Retrieves data from the Kolada API and deserializes the JSON response into the specified type T.
        /// </summary>
        /// <typeparam name="T">The type into which the JSON response is deserialized.</typeparam>
        /// <param name="endpoint">The relative URL of the endpoint to retrieve data from. This value should not include the base URL.</param>
        /// <returns>An instance of type T with the deserialized data from the endpoint.</returns>
        public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct)
        {
            // Resolves relative endpoints against BaseUrl; an absolute URL (e.g. next_url) is used as-is.
            var url = new Uri(new Uri(BaseUrl), endpoint);

            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogError(
                        "Kolada API Request Failed. URL: {Url} Status: {StatusCode} Response: {Response}",
                        url,
                        response.StatusCode,
                        errorBody
                        );

                    response.EnsureSuccessStatusCode(); // Throws HttpRequestException if Statuscode is NOT 200-299.
                }

                using var contentStream = await response.Content.ReadAsStreamAsync(ct);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return await JsonSerializer.DeserializeAsync<T>(contentStream, options, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error fetching from: {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Follows Kolada's next_url links and returns every page's values concatenated.
        /// Kolada caps a page at 5000 rows, so anything larger silently truncates without this.
        /// </summary>
        private async Task<List<T>> GetAllAsync<T>(string endpoint, CancellationToken ct)
        {
            var all = new List<T>();
            string? next = endpoint;
            var pages = 0;

            while (next is not null)
            {
                var page = await GetAsync<KoladaResponseDto<T>>(next, ct);
                if (page?.Values is null) break;

                all.AddRange(page.Values);
                pages++;

                // ponytail: guards against an API that hands back its own URL; no page cap, the data set decides the size.
                next = page.NextUrl == next ? null : page.NextUrl;
            }

            _logger.LogInformation("Fetched {Count} rows over {Pages} page(s) from {Endpoint}", all.Count, pages, endpoint);
            return all;
        }

        /// <summary>
        /// Asynchronously retrieves a list of municipalities from the Kolada API.
        /// </summary>
        /// <returns>A list of KoladaMunicipalityDto objects representing the municipalities. Returns an empty list if no
        /// municipalities are found.</returns>
        public async Task<List<KoladaMunicipalityDto>> GetMunicipalitiesAsync<T>(CancellationToken ct = default)
        {
            return await GetAllAsync<KoladaMunicipalityDto>("municipality", ct);
        }

        /// <summary>
        /// Asynchronously retrieves a list of KPIs from the Kolada API.
        /// </summary>
        /// <returns>A list of KoladaKpiDto objects representing the KPIs. Returns an empty list if no
        /// KPIs are found.</returns>
        public async Task<List<KoladaKpiDto>> GetKpisAsync<T>(CancellationToken ct = default)
        {
            return await GetAllAsync<KoladaKpiDto>("kpi", ct);
        }

        public async Task<List<KoladaFactDto>> GetFactAsync<T>(string[] kpiArray, string year, CancellationToken ct = default)
        {
            string kpis = String.Join(",", kpiArray);
            _logger.LogInformation("{Kpis}", kpis);
            var groups = await GetAllAsync<KoladaGroupResponse>($"data/kpi/{kpis}/year/{year}", ct);

            var resultList = new List<KoladaFactDto>();

            foreach (var group in groups)
            {
                if (group.InnerValues is null)
                {
                    continue;
                }

                foreach (var point in group.InnerValues)
                {
                    resultList.Add(new KoladaFactDto
                    (
                        Gender: point.Gender ?? "T",
                        Count: point.Count ?? 0,
                        Status: point.Status ?? string.Empty,
                        Value: point.Value ?? 0,
                        Isdeleted: false,
                        KpiKoladaId: group.Kpi,
                        Year: group.Period,
                        MunicipalityKoladaId: group.Municipality
                    ));
                }

            }

            return resultList;
        }
    }
}
