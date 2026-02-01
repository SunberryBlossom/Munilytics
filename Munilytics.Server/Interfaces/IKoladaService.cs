using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Models.DTOs;

namespace Munilytics.Server.Interfaces
{
    public interface IKoladaService
    {
        Task<T?> GetAsync<T>(string endpoint, CancellationToken ct);
        Task<List<KoladaMunicipalityDto>> GetMunicipalitiesAsync<T>(CancellationToken ct = default);
    }
}
