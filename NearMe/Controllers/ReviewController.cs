using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
using System.Linq.Expressions;
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

        private string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private static readonly Expression<Func<Review, ReviewResponseDto>> ToDto = r => new ReviewResponseDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            ProviderProfileId = r.ProviderProfileId,
            UserName = r.User.fullname
        };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var reviews = await _context.Reviews
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReviewResponseDto>> GetById(int id)
        {
            var review = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();

            return review == null ? NotFound() : Ok(review);
        }

        [HttpGet("provider/{id:int}")]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetByProvider(
            int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProviderProfileId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewResponseDto>> Create([FromBody] CreateReviewDto dto)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var provider = await _context.ProviderProfiles
                .AsNoTracking()
                .Select(p => new { p.Id, p.UserId })
                .FirstOrDefaultAsync(p => p.Id == dto.ProviderProfileId);

            if (provider == null)
                return NotFound("Provider not found.");

            if (provider.UserId == userId)
                return BadRequest("You cannot review your own profile.");

            var alreadyReviewed = await _context.Reviews.AnyAsync(r =>
                r.UserId == userId && r.ProviderProfileId == dto.ProviderProfileId);

            if (alreadyReviewed)
                return BadRequest("You have already reviewed this provider.");

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ProviderProfileId = dto.ProviderProfileId
            };

            _context.Reviews.Add(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // unique index (UserId, ProviderProfileId) hit by a concurrent request
                return BadRequest("You have already reviewed this provider.");
            }

            var result = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.Id == review.Id)
                .Select(ToDto)
                .FirstAsync();

            return CreatedAtAction(nameof(GetById), new { id = review.Id }, result);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteById(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            if (review.UserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}