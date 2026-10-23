using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
using System.Security.Claims;
namespace NearMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        public ReviewController(ApplicationDbContext context)
        {
            _context = context;

        }

        //git all 
        [HttpGet]
        public IActionResult GetAll()
        {
            var Reviews = _context.Reviews.ToList();
            return Ok(Reviews);
        }

        //git by id 
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var Review = _context.Reviews.Find(id);
            return Ok(Review);
        }
        [HttpGet("provider/{id:int}")]
        public IActionResult Getbyprovider(int id) 
        {
            // var ProviderProfile = _context.ProviderProfiles.Find(id);
            var reviews = _context.Reviews.Where(r => r.ProviderProfileId == id).ToList();
            return Ok(reviews);
        }
        //add new review
        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] CreateReviewDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            bool alreadyReviewed = _context.Reviews.Any(r =>
                r.UserId == userId &&
                r.ProviderProfileId == dto.ProviderProfileId);

            if (alreadyReviewed)
            {
                return BadRequest("You have already reviewed this provider.");
            }

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now,
                UserId = userId,  
                ProviderProfileId = dto.ProviderProfileId
            };

           
            _context.Reviews.Add(review);

           
            _context.SaveChanges();
   return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteById(int id)
        {
            var Review = _context.Reviews.Find(id);
           
            _context.Reviews.Remove(Review);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
