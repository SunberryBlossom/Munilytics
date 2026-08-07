using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Features.Admin.SyncMunicipalities.DTOs;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Models.DTOs;
using Wolverine;
using Wolverine.Attributes;

namespace Munilytics.Server.Features.Admin.SyncMunicipalities
{
    [LocalQueue("sync-kolada")]
    public record SyncMunicipalitiesCommand();
    public class SyncMunicipalities : EndpointWithoutRequest<SyncMunicipalitiesResponse>
    {
        private readonly IMessageBus _bus;

        public SyncMunicipalities(IMessageBus bus)
        {
            _bus = bus;
        }

        public override void Configure()
        {
            Post("/admin/sync/municipalities");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await _bus.SendAsync(new SyncMunicipalitiesCommand());
            await Send.AcceptedAtAsync("Municipalities is being processed");
        }
    }

    public static class SyncMunicipalitiesHandler
    {
        [Transactional]
        public static async Task Handle(SyncMunicipalitiesCommand cmd, MunilyticsDbContext db, IKoladaService koladaService, ILogger<SyncMunicipalities> logger, CancellationToken ct)
        {
            // Log that the sync process has started, and fetch all municipalities from the Kolada API
            logger.LogInformation("Started Municipalities Sync");
            var municipalities = await koladaService.GetMunicipalitiesAsync<KoladaMunicipalityDto>(ct);

            if (municipalities == null || municipalities.Count == 0)
            {
                logger.LogWarning("No municipalities fetched from Kolada API. Skipping...");
                return;
            }

            // Fetch the municipalities we already have in our Data Warehouse (DWH)
            var existingMunicipalities = await db.Dim_Municipalities.ToDictionaryAsync(m => m.KoladaId, m => m, ct);
            var newEntities = new List<DimMunicipality>();

            foreach (var dto in municipalities)
            {
                var county = DetermineCounty(dto.Id);
                var (skrCode, skrName) = DetermineSkrGroup(dto.Id);

                // If the current new DTO is an already existing municipality, update the matching entity in the database regarding it.
                if (existingMunicipalities.TryGetValue(dto.Id, out var existingIdentity))
                {
                        existingIdentity.Title = dto.Title;
                        existingIdentity.Type = dto.Type;
                        existingIdentity.County = county;
                        existingIdentity.SkrGroupCode = skrCode;
                        existingIdentity.SkrGroupName = skrName;
                }
                else
                {
                    // Otherwise create a new entity
                    var newEntity = new DimMunicipality
                    {
                        Title = dto.Title,
                        Type = dto.Type,
                        KoladaId = dto.Id,
                        County = county,
                        SkrGroupCode = skrCode,
                        SkrGroupName = skrName
                    };

                    newEntities.Add(newEntity);
                }
            }

            if (newEntities.Count != 0)
                {
                    await db.Dim_Municipalities.AddRangeAsync(newEntities, ct);
                }

            logger.LogInformation("Finished Municipalities Sync");
        }

        private static string DetermineCounty(string municipalityCode)
        {
            if (string.IsNullOrEmpty(municipalityCode) || municipalityCode.Length < 2)
                return "Unknown";

            string prefix = municipalityCode.Substring(0, 2);

            return prefix switch
            {
                "01" => "Stockholms län",
                "03" => "Uppsala län",
                "04" => "Södermanlands län",
                "05" => "Östergötlands län",
                "06" => "Jönköpings län",
                "07" => "Kronobergs län",
                "08" => "Kalmar län",
                "09" => "Gotlands län",
                "10" => "Blekinge län",
                "12" => "Skåne län",
                "13" => "Hallands län",
                "14" => "Västra Götalands län",
                "17" => "Värmlands län",
                "18" => "Örebro län",
                "19" => "Västmanlands län",
                "20" => "Dalarnas län",
                "21" => "Gävleborgs län",
                "22" => "Västernorrlands län",
                "23" => "Jämtlands län",
                "24" => "Västerbottens län",
                "25" => "Norrbottens län",
                _ => "Unknown"
            };
        }

        private static (string Code, string Name) DetermineSkrGroup(string municipalityCode)
        {
            // TODO: Need to find an API for this most likely, if we wish to use SKR groupings... maybe not MVP-material..?

            return ("UNK", "Unknown Group");
        }
    }
}