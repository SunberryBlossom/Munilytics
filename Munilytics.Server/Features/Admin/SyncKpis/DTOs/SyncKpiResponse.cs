using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Infrastructure.Persistence;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Interfaces;
using Munilytics.Server.Models.DTOs;
using Wolverine;


namespace Munilytics.Server.Features.Admin.SyncKpis.DTOs
{
    public record SyncKpiResponse
    {
        public string Message {get; set;} = string.Empty;
        public bool Success;
    }
}