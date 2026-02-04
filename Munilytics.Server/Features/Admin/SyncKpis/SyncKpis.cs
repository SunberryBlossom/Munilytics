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
    public record SyncKpiCommand();

    public class SyncKpis : EndpointWithoutRequest<SyncKpiResponse>
    {
        private readonly IMessageBus _bus;
        public SyncKpis (IMessageBus bus)
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
            try
            {
            var result = await _bus.InvokeAsync<SyncKpiResponse>(new SyncKpiCommand(), ct);
            await Send.OkAsync(result, ct);
            }
            catch (ApplicationException ex)
            {
                ThrowError(ex.Message);
            }
        }
    }

    public static class SyncKpisHandler
    {
        [Transactional]
        public static async Task<SyncKpiResponse> Handle(SyncKpiCommand cmd, MunilyticsDbContext db, IKoladaService koladaService, CancellationToken ct)
        {
            var kpis = await koladaService.GetKpisAsync<KoladaKpiDto>(ct);

            if (kpis == null || kpis.Count == 0)
            {
                throw new ApplicationException("For some reason the KoladaService returned 0 KPIs from the KOLADA database...");
            }

            var existingKpis = await db.Dim_KPIs.ToDictionaryAsync(k => k.KpiCode, k => k, ct);

            foreach(var dto in kpis)
            {
                if(existingKpis.TryGetValue(dto.Id, out var existingIdentity))
                {
                    if (existingIdentity.Title != dto.Title || existingIdentity.Description != dto.Description) // Could be made to check everything if we find it necessary.
                    {
                        existingIdentity.Title = dto.Title;
                        existingIdentity.Description = dto.Description ?? "";
                        existingIdentity.IsDividedByGender = dto.IsDividedByGender;
                        existingIdentity.MunicipalityType = dto.MunicipalityType;
                        existingIdentity.Auspice = dto.Auspice;
                        existingIdentity.OperatingArea = dto.OperatingArea;
                        existingIdentity.Perspective = dto.Perspective;
                    }
                }
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
                        Unit = "N/A"
                    };

                        db.Dim_KPIs.Add(newEntity);
                    }
            }

            return new SyncKpiResponse("KPI successfully synced", true );
        }
    }
}