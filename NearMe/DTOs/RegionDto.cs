using System.ComponentModel.DataAnnotations;

namespace NearMe.DTOs
{
    public class RegionDto
    {
        [Required, MaxLength(500)]
        public string RegionName { get; set; } = string.Empty;
    }
}