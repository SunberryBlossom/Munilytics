using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Models.DTOs;
using Wolverine;


namespace Munilytics.Server.Features.Admin.SyncKpis.DTOs
{
    public record SyncKpiResponse(string Message, bool Success);
}