namespace NearMe.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt {  get; set; }= DateTime.Now;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null;
        public int ProviderProfileId  { get; set; }
        public ProviderProfile providerProfile { get; set; } = null;

    }
}
