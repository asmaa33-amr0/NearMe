using System.ComponentModel.DataAnnotations;

namespace NearMe.DTOs
{
    public class CategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? IconPath { get; set; }
    }
}