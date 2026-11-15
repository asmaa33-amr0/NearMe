namespace NearMe.Models
{
    public class ProviderProfile
    {
        public int Id { get; set; }

       
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int RegionId { get; set; }
        public Region Region { get; set; } = null!;

         public string BusinessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;

        public string? Contact { get; set; }
        public ICollection<ProviderImage> Gallery { get; set; }
        public ICollection<Review> Reviews   { get; set; }
        public double Latitude { get;   set; }
        public double Longitude { get;   set; }
    }
}
