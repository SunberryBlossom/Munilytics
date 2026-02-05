using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolverine;
using FastEndpoints;
using Munilytics.Server.Features.Admin.SyncFactData.DTOs;
using Wolverine.Attributes;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Infrastructure.Kolada;
using Munilytics.Server.Models.DTOs;
using Munilytics.Server.Infrastructure.Kolada.DTOs;

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
            Get("/admin/fact");
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

            var existingFacts = db.Fact_KpiMeasurements.ToList();

            return new SyncFactDataResponse("Syncing of facts complete", true);
        }
    }

}