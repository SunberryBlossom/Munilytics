namespace Munilytics.Server.Interfaces
{
    public interface IKoladaService
    {
        Task<T> GetAsync<T>(string endpoint, CancellationToken ct);
    }
}
