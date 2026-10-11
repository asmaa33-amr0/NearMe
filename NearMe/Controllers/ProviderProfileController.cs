using Microsoft.AspNetCore.Mvc;
using NearMe.Data;

namespace NearMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProviderProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProviderProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}/GlobalRating")]
        public IActionResult GetGlobalRating(int id)
        {
            var providerExists = _context.ProviderProfiles.Any(p => p.Id == id);
            if (!providerExists)
            {
                return NotFound($"Provider profile with ID {id} was not found.");
            }

            var providerReviews = _context.Reviews.Where(r => r.ProviderProfileId == id);
            int totalReviews = providerReviews.Count();

            double averageRating = totalReviews > 0
                ? Math.Round(providerReviews.Average(r => r.Rating), 1)
                : 0.0;

            return Ok(new
            {
                ProviderProfileId = id,
                AverageRating = averageRating,
                TotalReviewsCount = totalReviews
            });
        }
    }
}
