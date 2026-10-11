namespace NearMe.Models
{
    public class ProviderImage
    {
        public int Id { get; set; } 
        public string ImageUrl { get; set; } = string.Empty;
        public int ProviderProfileId { get; set; }
        public ProviderProfile ProviderProfile { get; set; } = null;
    }
}
