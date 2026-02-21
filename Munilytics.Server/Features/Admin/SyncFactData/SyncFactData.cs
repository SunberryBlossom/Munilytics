using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Domain.Enums;
using Munilytics.Server.Features.Admin.SyncFactData.DTOs;
using Munilytics.Server.Infrastructure.Kolada;
using Munilytics.Server.Infrastructure.Kolada.DTOs;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using System.Linq;
using Wolverine;
using Wolverine.Attributes;

namespace Munilytics.Server.Features.Admin.SyncFactData
{
    [LocalQueue("sync-kolada")]
    public record SyncFactCommand(string[] Kpis, int Year);
    public record SyncAllFactsCommand(int StartYear = 1994, int? EndYear = null);
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
    // ------------------------- ETL FOR FACT TABLE -----------------------------

    logger.LogInformation("Starting Fact sync for year: {Year}. Current batch: {BatchSize}", cmd.Year, cmd.Kpis.Length);

    // EXTRACT
    // Fetches the new facts from Kolada API into a list of DTOs. Checks to see if this was successful.
    var newFacts = await koladaService.GetFactAsync<KoladaFactDto>(cmd.Kpis, cmd.Year.ToString(), ct);
    if (newFacts is null || newFacts.Count == 0)
    {
        logger.LogWarning("No facts found in {Year}. Skipping...", cmd.Year);
        return;
    }

    // Mapping out municipalities, KPIs and Genders to dictionaries from our data warehouse (DWH)
    var municipalitiesMap = await db.Dim_Municipalities.ToDictionaryAsync(m => m.KoladaId, m => m.Id, ct);

    var kpisMap = await db.Dim_KPIs
        .Where(k => cmd.Kpis.Contains(k.KpiCode))
        .ToDictionaryAsync(k => k.KpiCode, k => k.Id, ct);

    var gendersList = await db.Dim_Gender.ToListAsync(ct);
    var gendersMap = gendersList.ToDictionary(g => g.Code.ToString(), g => g.Id);

    // fetch the time ID for the year we are currently working with
    var dimTimeId = await db.Dim_Time
        .Where(t => t.Year == cmd.Year)
        .Select(t => t.Id)
        .FirstOrDefaultAsync(ct);

    var now = DateTime.UtcNow;

    if (dimTimeId == 0)
    {
        logger.LogError("Unknown year in db: {Year}. Is your database updated with the latest seed data?", cmd.Year);
        return;
    }

    // this is because we use Columnar on the fact table:
    // first make an array of the KPI Ids that we already have in our DWH, then delete those KPIs from it, for the current year that we are processing.
    var kpiIdsToClear = kpisMap.Values.ToArray();
    await db.Database.ExecuteSqlRawAsync(
        "DELETE FROM \"Fact_KpiMeasurements\" WHERE \"DimTimeId\" = {0} AND \"DimKpiId\" = ANY({1})",
        new object[] { dimTimeId, kpiIdsToClear }, ct);


    // Create a list to hold all the new entities we wanna add into the DWH
    var newEntities = new List<FactKpiMeasurement>(newFacts.Count);

    // Go through each and every DTO that holds the new facts we fetched from Kolada...
    foreach (var dto in newFacts)
    {
        // ...If the municipality, or the KPI is unknown, skip the DTO and go to the next.
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

        // fetch a genderKey. If the DTOs gender has nothing in it, set it as T. Otherwise, use it as it was (most likely T, K or M instead of omitted)
        var genderKey = string.IsNullOrWhiteSpace(dto.Gender) ? "T" : dto.Gender;

        // checks if the database DOES NOT have the gendercode provided from the DTO (even after the ternary), log a warning and skip...
        if (!gendersMap.TryGetValue(genderKey, out var genderId))
        {
            logger.LogWarning("Unknown gender code '{Gender}'.", dto.Gender);
            continue;
        }

        // TRANSFORM
        // Otherwise, create a new fact for the database
        var rawValue = (decimal)dto.Value;
        var normalizedValue = KoladaSyncAgent.NormalizeDashboardValueByKpiCode(dto.KpiKoladaId, rawValue);

        newEntities.Add(new FactKpiMeasurement
        {
            Value = normalizedValue,
            Count = dto.Count,
            Status = dto.Status,
            DimMunicipalityId = municipalityId,
            DimKpiId = kpiId,
            DimGenderId = genderId,
            DimTimeId = dimTimeId,
            ImportDate = now,
            LatestUpdate = now
        });
    }

    // LOAD
    // Final security check: This block only runs if we actually added something to our list of new entities.
    if (newEntities.Count != 0)
    {
        await db.Fact_KpiMeasurements.AddRangeAsync(newEntities, ct);
    }

    // Final log to tell us everything worked
    logger.LogInformation("Finished Fact sync for year: {Year}. Added {Count} records.", cmd.Year, newEntities.Count);
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
            var kpiBatches = kpis.Chunk(10).ToArray();
            var endYear = cmd.EndYear ?? DateTime.UtcNow.Year;

            for (int year = cmd.StartYear; year <= endYear; year++)
            {
                foreach (string[] kpiBatch in kpiBatches)
                {
                    await bus.SendAsync(new SyncFactCommand(kpiBatch, year));
                }
            }
        }
    }
}