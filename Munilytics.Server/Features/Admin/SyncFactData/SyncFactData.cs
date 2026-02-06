using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Domain.Enums;
using Munilytics.Server.Features.Admin.SyncFactData.DTOs;
using Munilytics.Server.Infrastructure.Kolada.DTOs;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using System.Linq;
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
        public static async Task Handle(SyncFactCommand cmd, CancellationToken ct, MunilyticsDbContext db, ILogger logger, IKoladaService koladaService)
        {
            logger.LogInformation($"Starting Fact sync for year: {cmd.year} and municipality id: {cmd.municipalityKoladaId}");

            var newFacts = await koladaService.GetFactAsync<KoladaFactDto>(cmd.year.ToString(), cmd.municipalityKoladaId, ct);

            if (newFacts is null || newFacts.Count == 0)
            {
                logger.LogWarning($"No facts found for {cmd.municipalityKoladaId} in {cmd.year}");
                throw new ApplicationException($"No facts found for {cmd.municipalityKoladaId} in {cmd.year}");
            }

            var municipality = await db.Dim_Municipalities
                .FirstOrDefaultAsync(m => m.KoladaId == cmd.municipalityKoladaId, ct);

            if (municipality == null)
            {
                throw new ApplicationException($"Municipality {cmd.municipalityKoladaId} not found in database");
            }

            //For MVP i will only check Municipality ID. For real production we would need to compare all rows
            var kpisByCode = await db.Dim_KPIs
                .ToDictionaryAsync(k => k.KpiCode, k => k.Id, ct);

            var gendersByCode = await db.Dim_Gender
                .ToDictionaryAsync(g => g.Code, g => g.Id, ct);

            var existingFacts = await db.Fact_KpiMeasurements
                .Where(f => f.DimMunicipalityId == municipality.Id && f.DimTime.Year == cmd.year)
                .Include(f => f.DimKpi)
                .ToListAsync();

            var factMap = existingFacts
                .ToDictionary(f => (KpiId: f.DimKpiId, GenderId: f.DimGenderId), f => f);


            foreach (var dto in newFacts)
            {
                if (!kpisByCode.TryGetValue(dto.KpiKoladaId, out var kpiId))
                {
                    continue;
                }

                var genderString = string.IsNullOrWhiteSpace(dto.Gender) ? "T" : dto.Gender;

                if (!Enum.TryParse<GenderCode>(genderString, true, out var genderCode) ||
                    !gendersByCode.TryGetValue(genderCode, out var genderId))
                {
                    logger.LogWarning($"Unknown gender code '{dto.Gender}'.");
                    continue;
                }

                var lookupKey = (KpiId: kpiId, GenderId: genderId);

                if (factMap.TryGetValue(lookupKey, out var existingEntity))
                {
                    existingEntity.Value = dto.Value;
                    existingEntity.Count = dto.Count;
                    existingEntity.LatestUpdate = DateTime.UtcNow;
                    existingEntity.Status = dto.Status;
                }
                else
                {
                    var newEntity = new FactKpiMeasurement
                    {
                        Value = dto.Value,
                        Count = dto.Count,
                        ImportDate = DateTime.UtcNow,
                        LatestUpdate = DateTime.UtcNow,
                        Status = dto.Status,

                        // FK
                        DimMunicipalityId = municipality.Id,
                        DimKpiId = kpiId,
                        DimGenderId = genderId,

                        DimTime = new DimTime
                        {
                            Year = dto.Year
                        },
                    };

                    db.Fact_KpiMeasurements.Add(newEntity);
                }
            }
            logger.LogInformation($"Finished Fact sync for year: {cmd.year} and municipality id: {cmd.municipalityKoladaId}");
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