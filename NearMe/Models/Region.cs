namespace NearMe.Models
{
    public class Region
    {
        public int id { get; set; }
        public string name { get; set; }=string.Empty;
        public ICollection<ProviderProfile> providers { get; set; } = new List<ProviderProfile>();
    }

   
}
