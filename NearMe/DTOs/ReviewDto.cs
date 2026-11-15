using System.ComponentModel.DataAnnotations;

namespace NearMe.DTOs
{
    public class CreateReviewDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ProviderProfileId { get; set; }
    }

    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ProviderProfileId { get; set; }
        public string? UserName { get; set; }
    }
    public class UpdateReviewDto
    {
        [Range(1, 5)] public int Rating { get; set; }
        [StringLength(1000)] public string? Comment { get; set; }
    }
}