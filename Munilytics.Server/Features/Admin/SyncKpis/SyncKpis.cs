using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Models.DTOs;
using Wolverine;
using FastEndpoints;
using Munilytics.Server.Features.Admin.SyncKpis.DTOs;
using Wolverine.Attributes;

namespace Munilytics.Server.Features.Admin.SyncKpis
{
    [LocalQueue("sync-kolada")]
    public record SyncKpiCommand();
    public class SyncKpis : EndpointWithoutRequest<SyncKpiResponse>
    {
        private readonly IMessageBus _bus;
        public SyncKpis(IMessageBus bus)
        {
            _bus = bus;
        }

        public override void Configure()
        {
            Post("/admin/sync/kpis");
            AllowAnonymous(); // Not sure if this should be anon or not... future work I guess
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await _bus.SendAsync(new SyncKpiCommand());
            await Send.AcceptedAtAsync("Syncing KPI in the background", cancellation: ct);
        }
    }

    public static class SyncKpisHandler
    {
        [Transactional]
        public static async Task Handle(SyncKpiCommand cmd, MunilyticsDbContext db, IKoladaService koladaService, ILogger<SyncKpis> logger, CancellationToken ct)
        {
            logger.LogInformation("Started KPI Sync");

            // Fetches the new KPIs from Kolada, saves them as a list of DTOs. If it is empty, send a warning in the log and stop the Handle.
            var newKpis = await koladaService.GetKpisAsync<KoladaKpiDto>(ct);
            if (newKpis == null || newKpis.Count == 0)
            {
                logger.LogWarning("No KPIs found. Skipping...");
                return;
            }

            // Fetches the KPIs that already exist in our data warehouse (DWH)
            var existingKpis = await db.Dim_KPIs.ToDictionaryAsync(k => k.KpiCode, k => k, ct);

            var newEntities = new List<DimKpi>();

            // loop through all the DTOs loaded into our list of new KPIs
            foreach (var dto in newKpis)
            {
                string parsedUnit = DetermineUnit(dto.Title, logger);

                // If this KPI already exists in our DWH, just update it's values and EF core will automatically do the rest, otherwise create a new object and add it.
                if (existingKpis.TryGetValue(dto.Id, out var existingIdentity))
                {
                        existingIdentity.Title = dto.Title;
                        existingIdentity.Description = dto.Description ?? "";
                        existingIdentity.IsDividedByGender = dto.IsDividedByGender;
                        existingIdentity.MunicipalityType = dto.MunicipalityType;
                        existingIdentity.Auspice = dto.Auspice;
                        existingIdentity.OperatingArea = dto.OperatingArea;
                        existingIdentity.Perspective = dto.Perspective;
                        existingIdentity.Unit = parsedUnit;
                }
                // If it isn't a new KPI, create it
                else
                {
                    var newEntity = new DimKpi
                    {
                        KpiCode = dto.Id,
                        Title = dto.Title,
                        Description = dto.Description ?? "",
                        IsDividedByGender = dto.IsDividedByGender,
                        MunicipalityType = dto.MunicipalityType,
                        Auspice = dto.Auspice,
                        OperatingArea = dto.OperatingArea,
                        Perspective = dto.Perspective,
                        Unit = parsedUnit
                    };

                    // Add the new KPI entity into our list
                    newEntities.Add(newEntity);
                }
            }
                if (newEntities.Count != 0)
                {
                    // Add all the new KPI entities to our DWH
                    await db.Dim_KPIs.AddRangeAsync(newEntities, ct);
                }

            logger.LogInformation("Finished background job for KPI sync");
        }

        // Helper method to try and parse out the Unit from the title
        private static string DetermineUnit(string title, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                logger.LogWarning("Could not parse unit since title is null or whitespace. Title value: '{Title}'", title);
                return "N/A";
            }

            var t = title.ToLowerInvariant();

            if (t.Contains("(%)") || t.Contains("procent") || t.Contains("andel")) return "%";
            if (t.Contains("(kr)") || t.Contains("kronor") || t.Contains("kostnad")) return "SEK";
            if (t.Contains("mnkr")) return "mnkr";
            if (t.Contains("antal") || t.Contains("styck")) return "Antal";
            if (t.Contains("ton")) return "Ton";
            if (t.Contains("kwh")) return "kWh";

            return "N/A";
        }
    }
}