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
    public record SyncFactCommand(string[] kpis, int year);
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
            logger.LogInformation("Starting Fact sync for year: {Year}. Current batch: {BatchSize}", cmd.year, cmd.kpis.Length);

            // Get facts for current batch and current year for all municipalities
            var newFacts = await koladaService.GetFactAsync<KoladaFactDto>(cmd.kpis, cmd.year.ToString(), ct);

            if (newFacts is null || newFacts.Count == 0)
            {
                logger.LogWarning("No facts found for in {Year}. Skipping...", cmd.year);
                return;
            }

            // Map all inputs needed for later.
            var municipalitiesMap = await db.Dim_Municipalities
                .ToDictionaryAsync(m => m.KoladaId, m => m.Id, ct);

            var kpisMap = await db.Dim_KPIs
                .Where(k => cmd.kpis.Contains(k.KpiCode))
                .ToDictionaryAsync(k => k.KpiCode, k => k.Id, ct);

            var gendersMap = await db.Dim_Gender
                .ToDictionaryAsync(g => g.Code, g => g.Id, ct);

            var dimTimeId = await db.Dim_Time
                .Where(t => t.Year == cmd.year)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(ct);

            if (dimTimeId == 0)
            {
                logger.LogError("Unknown year in db: {Year}. Is your database updated with the latest seed data?", cmd.year);
                return;
            }

            // Load existing facts for this year and for this kpi batch
            var existingFacts = await db.Fact_KpiMeasurements
                .Where(f => f.DimTimeId == dimTimeId && kpisMap.Values.Contains(f.DimKpiId))
                .ToListAsync();

            // Lookup table where we use composite key of municipality, kpiid and genderid to get a fact entity
            var factMap = existingFacts
                .ToDictionary(f => (MunicipalityId: f.DimMunicipalityId, KpiId: f.DimKpiId, GenderId: f.DimGenderId), f => f);

            // Use a list to bulk add to dbcontext
            var newEntities = new List<FactKpiMeasurement>();

            foreach (var dto in newFacts)
            {
                // Skip this if this municipality or koladaid doesnt exist in our database
                // Otherwise we give back the specific id for both.
                // These will log, so can be a good idea to add the logged ones in the future
                if (!municipalitiesMap.TryGetValue(dto.MunicipalityKoladaId, out var municipalityId))
                {
                    logger.LogDebug("Skipping Unknown Municipality: {MunicipalityId}", dto.MunicipalityKoladaId);
                    continue;
                }

                if (!kpisMap.TryGetValue(dto.KpiKoladaId, out var kpiId))
                {
                    logger.LogTrace("Skipping unseeded KPI: {KpiId}", dto.KpiKoladaId);
                    continue;
                }

                var genderString = string.IsNullOrWhiteSpace(dto.Gender) ? "T" : dto.Gender;

                if (!Enum.TryParse<GenderCode>(genderString, true, out var genderCode) ||
                    !gendersMap.TryGetValue(genderCode, out var genderId))
                {
                    logger.LogWarning("Unknown gender code '{Gender}'.", dto.Gender);
                    continue;
                }

                var factKey = (MunicipalityId: municipalityId, KpiId: kpiId, GenderId: genderId);

                if (factMap.TryGetValue(factKey, out var existingEntity))
                {
                    bool hasChanged =
                        existingEntity.Value != (decimal)dto.Value ||
                        existingEntity.Count != dto.Count ||
                        existingEntity.Status != dto.Status;

                    if (hasChanged)
                    {
                        existingEntity.Value = (decimal)dto.Value;
                        existingEntity.Count = dto.Count;
                        existingEntity.Status = dto.Status;

                        existingEntity.LatestUpdate = DateTime.UtcNow;
                    }
                }
                else
                {
                    var newEntity = new FactKpiMeasurement
                    {
                        Value = dto.Value,
                        Count = dto.Count,
                        Status = dto.Status,

                        // FKs
                        DimMunicipalityId = municipalityId,
                        DimKpiId = kpiId,
                        DimGenderId = genderId,
                        DimTimeId = dimTimeId,

                        ImportDate = DateTime.UtcNow,
                        LatestUpdate = DateTime.UtcNow,
                    };
                    newEntities.Add(newEntity);
                }
            }
            if (newEntities.Count != 0)
            {
                await db.Fact_KpiMeasurements.AddRangeAsync(newEntities);
            }

            logger.LogInformation("Finished Fact sync for year: {Year} with batch size {BatchSize}", cmd.year, cmd.kpis.Length);
        }
    }

    public static class SyncAllFactsHandler
    {
        public static async Task Handle(SyncAllFactsCommand cmd, CancellationToken ct, MunilyticsDbContext db, IMessageBus bus)
        {
            var kpis = await db.Dim_KPIs
                .Select(k => k.KpiCode)
                .ToArrayAsync(ct);

            // This batches all kpis into 10 arrays of kpis.
            // Note that Kolada has a maximum of 25 members.
            // However since some KPIs can be quite big, this needs to be lower so Kolada doesnt complain
            var kpiBatches = kpis.Chunk(10);

            for (int year = 1994; year <= DateTime.Now.Year; year++)
            {
                foreach (string[] kpiBatch in kpiBatches)
                {
                    await bus.SendAsync(new SyncFactCommand(kpiBatch, year));
                }
            }
        }
    }
}