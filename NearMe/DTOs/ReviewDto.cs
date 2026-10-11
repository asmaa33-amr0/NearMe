namespace NearMe.DTOs
{
    public class ReviewDto
    {
    }

    public class CreateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int ProviderProfileId { get; set; }
    }
}
