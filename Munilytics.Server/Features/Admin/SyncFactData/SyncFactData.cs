using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Domain.Enums;
using Munilytics.Server.Features.Admin.SyncFactData.DTOs;
using Munilytics.Server.Infrastructure.Kolada.DTOs;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using Wolverine;
using Wolverine.Attributes;

namespace Munilytics.Server.Features.Admin.SyncFactData
{
    [LocalQueue("sync-kolada")]
    public record SyncFactCommand(int year, string municipalityKoladaId);
    public record SyncAllFactsCommand();
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
            await _bus.SendAsync(new SyncAllFactsCommand());
            await Send.AcceptedAtAsync("Syncing Fact Data in the background", cancellation: ct);
        }
    }

    public static class SyncFactHandler
    {
        [Transactional]
        public static async Task<SyncFactDataResponse> Handle (SyncFactCommand cmd, CancellationToken ct, MunilyticsDbContext db, IKoladaService koladaService)
        {
            var newFacts = await koladaService.GetFactAsync<KoladaFactDto>(cmd.year.ToString(), cmd.municipalityKoladaId, ct);

            if (newFacts is null || newFacts.Count == 0)
            {
                throw new ApplicationException("Could not find any facts from Kolada. Something must be wrong");
            }

            //For MVP i will only check Municipality ID. For real production we would need to compare all rows
            var existingFacts = await db.Fact_KpiMeasurements.ToDictionaryAsync(f => f.DimMunicipalityId.ToString(), f => f, ct);
            var gendersByCode = await db.Dim_Gender.ToDictionaryAsync(g => g.Code, g => g.Id, ct);

            foreach (var dto in newFacts)
            {
                var genderString = string.IsNullOrWhiteSpace(dto.Gender) ? "T" : dto.Gender;

                var municipality = await db.Dim_Municipalities
                    .FirstOrDefaultAsync(m => m.KoladaId == dto.MunicipalityKoladaId);
                var kpi = await db.Dim_KPIs
                    .FirstOrDefaultAsync(k => k.KpiCode == dto.KpiKoladaId);

                if (kpi is null)
                {
                    continue;
                }

                if (!Enum.TryParse<GenderCode>(genderString, true, out var genderCode) ||
                    !gendersByCode.TryGetValue(genderCode, out var genderId))
                {
                    throw new ApplicationException($"Unknown gender code '{dto.Gender}'.");
                }

                if (existingFacts.TryGetValue(dto.MunicipalityKoladaId, out var existingIdentity))
                {
                    existingIdentity.Value = dto.Value;
                    existingIdentity.Count = dto.Count;
                    existingIdentity.LatestUpdate = DateTime.UtcNow;
                    existingIdentity.DimTime = new DimTime
                    {
                        Year = dto.Year
                    };
                    existingIdentity.DimGenderId = genderId;
                    existingIdentity.Status = dto.Status;
                    existingIdentity.DimKpiId = kpi.Id;
                }
                else
                {

                    var newEntity = new FactKpiMeasurement
                    {
                        Value = dto.Value,
                        Count = dto.Count,
                        ImportDate = DateTime.UtcNow,
                        LatestUpdate = DateTime.UtcNow,
                        DimTime = new DimTime
                        {
                            Year = dto.Year
                        },
                        DimGenderId = genderId,
                        Status = dto.Status,

                        DimMunicipalityId = municipality.Id,
                        DimKpiId = kpi.Id
                    };

                    db.Fact_KpiMeasurements.Add(newEntity);
                }
            }

            return new SyncFactDataResponse("Syncing of facts complete", true);
        }
    }

    public static class SyncAllFactsHandler
    {
        public static async Task Handle(SyncAllFactsCommand cmd, CancellationToken ct, MunilyticsDbContext db, IMessageBus bus)
        {
            var municipalities = await db.Dim_Municipalities
                .Select(m => m.KoladaId)
                .ToListAsync();

            for (int i = 1994; i <= DateTime.Now.Year; i++)
            {
                foreach (var m in municipalities)
                {
                    await bus.SendAsync(new SyncFactCommand(i, m));
                }
            }
        }
    }   
}