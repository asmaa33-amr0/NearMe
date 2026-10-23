using NearMe.Models;

namespace NearMe.DTOs
{


    public class ProviderResponseDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }

        public CategoryDto category { get; set; } = null!;
        public string Region { get; set; } = null!;

         
        public ICollection<providerImageDTO> Gallery { get; set; } = new List<providerImageDTO>();
    }
    public class UpsertProviderDto
    {
        public string BusinessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int RegionId { get; set; }
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
    }

    public class CreateProviderProfileDto
    {
        public int CategoryId { get; set; }

        public int RegionId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Address { get; set; } = string.Empty;
    }

   
        public class RegisterDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }

        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
 
 
