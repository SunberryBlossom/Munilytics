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
            var gendersByCode = await db.Dim_Gender.ToDictionaryAsync(g => g.Code, g => g.Id, ct);

            foreach (var dto in newFacts)
            {
                if (!Enum.TryParse<GenderCode>(dto.Gender, true, out var genderCode) ||
                    !gendersByCode.TryGetValue(genderCode, out var genderId))
                {
                    throw new ApplicationException($"Unknown gender code '{dto.Gender}'.");
                }

                if (existingFacts.TryGetValue(dto.MunicipalityId, out var existingIdentity))
                {
                    existingIdentity.Value = dto.Value;
                    existingIdentity.Count = dto.Count;
                    existingIdentity.LatestUpdate = DateTime.Now;
                    existingIdentity.DimTime = new DimTime
                    {
                        Year = dto.Year
                    };
                    existingIdentity.DimGenderId = genderId;
                    existingIdentity.Status = dto.Status;
                }
                else
                {
                    var newEntity = new FactKpiMeasurement
                    {
                        Value = dto.Value,
                        Count = dto.Count,
                        ImportDate = DateTime.Today,
                        LatestUpdate = DateTime.Now,
                        DimMunicipalityId = dto.MunicipalityId,
                        DimKpiId = dto.KpiId,
                        DimTime = new DimTime
                        {
                            Year = dto.Year
                        },
                        DimGenderId = genderId,
                        Status = dto.Status
                    };

                    db.Fact_KpiMeasurements.Add(newEntity);
                }
            }

            return new SyncFactDataResponse("Syncing of facts complete", true);
        }
    }

}