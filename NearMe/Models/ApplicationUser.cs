using Microsoft.AspNetCore.Identity;

namespace NearMe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? fullname { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProviderProfile? ProviderProfile { get; set; }   
    }
}
