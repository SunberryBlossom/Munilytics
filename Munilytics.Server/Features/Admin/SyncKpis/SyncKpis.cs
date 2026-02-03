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
            AllowAnonymous();
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
        public static async Task Handle(SyncKpiCommand cmd, MunilyticsDbContext db, IKoladaService koladaService, CancellationToken ct)
        {
            var kpis = await koladaService.GetKpisAsync<KoladaKpiDto>(ct);
        }
    }
}