using System.ComponentModel.DataAnnotations;

namespace NearMe.DTOs
{
    public class ProviderImageDTO
    {
        [Required, MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
    }
    public class ProviderImageResponseDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int ProviderProfileId { get; set; }
    }
}