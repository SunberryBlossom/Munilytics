using Munilytics.Server.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Munilytics.Server.Domain.Entities
{
    public class DimGender
    {
        [Key]
        public GenderCode Code { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public int SortOrder { get; set; } 
    }
}
