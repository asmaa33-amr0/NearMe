namespace NearMe.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconPath { get; set; }

        public ICollection<ProviderProfile>profiles { get; set; }=new List<ProviderProfile>();
    }
}
