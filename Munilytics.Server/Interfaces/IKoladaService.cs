using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Infrastructure.Kolada.DTOs;
using Munilytics.Server.Models.DTOs;

namespace Munilytics.Server.Interfaces
{
    public interface IKoladaService
    {
        Task<T?> GetAsync<T>(string endpoint, CancellationToken ct);
        Task<List<KoladaMunicipalityDto>> GetMunicipalitiesAsync<T>(CancellationToken ct = default);
        Task<List<KoladaKpiDto>> GetKpisAsync<T>(CancellationToken ct = default);
        Task<List<KoladaFactDto>> GetFactAsync<T>(string year, string municipalityKoladaId, CancellationToken ct = default);
    }
}
