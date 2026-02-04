using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Munilytics.Server.Features.Admin.SyncMunicipalities.DTOs
{
    public record SyncMunicipalitiesResponse(string Message, bool Success);
}