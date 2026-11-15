using System.ComponentModel.DataAnnotations;

namespace NearMe.DTOs
{
    public class ProviderResponseDto
    {
        public string? Contact { get; set; }
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public CategoryDto Category { get; set; } = null!;
        public string Region { get; set; } = string.Empty;
        public ICollection<ProviderImageDTO> Gallery { get; set; } = new List<ProviderImageDTO>();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class ProviderListItemDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public string Contact { get;     set; }
        public double Latitude { get;    set; }
        public double Longitude { get; set; }
    }

    public class CreateProviderProfileDto
    {
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int RegionId { get; set; }

        [Required, MaxLength(200)]
        public string BusinessName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required, MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        [StringLength(300)]
        public string? Contact { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}