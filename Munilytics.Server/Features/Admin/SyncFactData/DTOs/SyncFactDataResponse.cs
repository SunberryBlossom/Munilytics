using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Munilytics.Server.Features.Admin.SyncFactData.DTOs
{
    public record SyncFactDataResponse(string Message, bool Success);
}