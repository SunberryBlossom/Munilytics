using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolverine;
using FastEndpoints;
using Munilytics.Server.Features.Admin.SyncMunicipalities.DTOs;
using Munilytics.Server.Features.Admin.SyncKpis.DTOs;
using Wolverine.Attributes;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;

namespace Munilytics.Server.Features.Admin.SyncMunicipalities
{
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
            try
            {
                var result = await _bus.InvokeAsync<SyncMunicipalitiesResponse>(new SyncMunicipalitiesCommand(), ct);
                await Send.OkAsync(result, ct);
            }
            catch (ApplicationException ex)
            {
                ThrowError(ex.Message);
            }
        }
    }

    public static class SyncMunicipalitiesHandler
    {
        [Transactional]
        public static async Task<SyncMunicipalitiesResponse> Handle(SyncMunicipalitiesCommand cmd, MunilyticsDbContext db, IKoladaService koladaService, CancellationToken ct)
        {
            var municipalities = await koladaService.GetMunicipalitiesAsync<KoladaMunicipalityDto>(ct);

            if (municipalities == null || municipalities.Count == 0)
            {
                throw new ApplicationException("No municipalities could be fetched from the Kolada API.");
            }

            var existingMunicipalities = await db.Dim_Municipalities.ToDictionaryAsync(m => m.Title, m => m, ct);

            foreach (var dto in municipalities)
            {
                if (existingMunicipalities.TryGetValue(dto.Title, out var existingIdentity))
                {
                    if(existingIdentity.Title != dto.Title)
                    {
                        existingIdentity.Title = dto.Title;
                        existingIdentity.Type = dto.Type;
                    }
                }
                else
                {
                    var newEntity = new DimMunicipality
                    {
                        Title = dto.Title,
                        Type = dto.Type
                    };

                    db.Dim_Municipalities.Add(newEntity);
                }
            }

            return new SyncMunicipalitiesResponse("Municipalities successfully synced", true);
        }
    }
}