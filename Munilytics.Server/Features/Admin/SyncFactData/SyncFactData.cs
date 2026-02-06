using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Features.Admin.SyncFactData.DTOs;
using Munilytics.Server.Infrastructure.Kolada.DTOs;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using Wolverine;
using Wolverine.Attributes;

namespace Munilytics.Server.Features.Admin.SyncFactData
{
    public record SyncFactCommand();
    public class SyncFactData : EndpointWithoutRequest<SyncFactDataResponse>
    {
        private readonly IMessageBus _bus;

        public SyncFactData(IMessageBus bus)
        {
            _bus = bus;
        }

        public override void Configure()
        {
            Post("/admin/sync/facts");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var result = await _bus.InvokeAsync<SyncFactDataResponse>(new SyncFactCommand(), ct);
                await Send.OkAsync(result, ct);
            }
            catch (ApplicationException ex)
            {
                ThrowError(ex.Message);
            }
        }
    }

    public static class SyncFactDataHandler
    {
        [Transactional]
        public static async Task<SyncFactDataResponse> Handle (SyncFactCommand cmd, CancellationToken ct, MunilyticsDbContext db, IKoladaService koladaService)
        {
            var newFacts = await koladaService.GetFactAsync<KoladaFactDto>(ct);

            if (newFacts is null || newFacts.Count == 0)
            {
                throw new ApplicationException("Could not find any facts from Kolada. Something must be wrong");
            }

            //For MVP i will only check Municipality ID. For real production we would need to compare all rows
            var existingFacts = await db.Fact_KpiMeasurements.ToDictionaryAsync(f => f.DimMunicipalityId, f => f, ct);

            foreach (var dto in newFacts)
            {
                if (existingFacts.TryGetValue(dto.MunicipalityId, out var existingIdentity))
                {
                    existingIdentity.Value = dto.Value;
                    existingIdentity.Count = dto.Count;
                    existingIdentity.LatestUpdate = DateTime.Now;

                    /* Values from JSON that we don't have in our facttable yet:
                     * Gender (This is it's own dimension but dont know how to get a Gender string into entire object)
                     * Status
                     * IsDeleted
                     * Period
                    */
                }
                else
                {
                    var newEntity = new FactKpiMeasurement
                    {
                        Value = dto.Value,
                        Count = dto.Count,
                        ImportDate = DateTime.Now,
                        LatestUpdate = DateTime.Now,
                        DimMunicipalityId = dto.MunicipalityId,
                        DimKpiId = dto.KpiId,
                    };

                    db.Fact_KpiMeasurements.Add(newEntity);
                }
            }

            return new SyncFactDataResponse("Syncing of facts complete", true);
        }
    }

}