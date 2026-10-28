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
    public class ProviderProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProviderProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private static readonly Expression<Func<ProviderProfile, ProviderListItemDto>> ToListItem = p => new ProviderListItemDto
        {
            Id = p.Id,
            BusinessName = p.BusinessName,
            Address = p.Address,
            CategoryName = p.Category.Name,
            RegionName = p.Region.name,
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
            ReviewsCount = p.Reviews.Count()
        };

        private static readonly Expression<Func<ProviderProfile, ProviderResponseDto>> ToDetails = p => new ProviderResponseDto
        {
            Id = p.Id,
            BusinessName = p.BusinessName,
            Address = p.Address,
            Description = p.Description,
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
            ReviewsCount = p.Reviews.Count(),
            Category = new CategoryDto { Name = p.Category.Name, IconPath = p.Category.IconPath },
            Region = p.Region.name,
            Gallery = p.Gallery.Select(img => new ProviderImageDTO { ImageUrl = img.ImageUrl }).ToList()
        };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProviderListItemDto>>> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var result = await _context.ProviderProfiles
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToListItem)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProviderResponseDto>> GetById(int id)
        {
            var provider = await _context.ProviderProfiles
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(ToDetails)
                .FirstOrDefaultAsync();

            return provider == null ? NotFound() : Ok(provider);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProviderListItemDto>>> Search(
            [FromQuery] string? name,
            [FromQuery] int? categoryId,
            [FromQuery] int? regionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.ProviderProfiles.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.BusinessName.Contains(name));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (regionId.HasValue)
                query = query.Where(p => p.RegionId == regionId.Value);

            var result = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToListItem)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<ProviderResponseDto>> GetMyProfile()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var provider = await _context.ProviderProfiles
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(ToDetails)
                .FirstOrDefaultAsync();

            return provider == null
                ? NotFound("You don't have a provider profile yet.")
                : Ok(provider);
        }

        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<ProviderResponseDto>> Create([FromBody] CreateProviderProfileDto dto)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (await _context.ProviderProfiles.AnyAsync(p => p.UserId == userId))
                return Conflict("You already have a provider profile.");

            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                return BadRequest("Category does not exist.");

            if (!await _context.Regions.AnyAsync(r => r.id == dto.RegionId))
                return BadRequest("Region does not exist.");

            var provider = new ProviderProfile
            {
                UserId = userId,
                BusinessName = dto.BusinessName,
                Address = dto.Address,
                RegionId = dto.RegionId,
                CategoryId = dto.CategoryId,
                Description = dto.Description
            };

            _context.ProviderProfiles.Add(provider);
            await _context.SaveChangesAsync();

            var result = await _context.ProviderProfiles
                .AsNoTracking()
                .Where(p => p.Id == provider.Id)
                .Select(ToDetails)
                .FirstAsync();

            return CreatedAtAction(nameof(GetById), new { id = provider.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<ProviderResponseDto>> Edit(int id, [FromBody] CreateProviderProfileDto dto)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var provider = await _context.ProviderProfiles.FirstOrDefaultAsync(p => p.Id == id);
            if (provider == null) return NotFound("Provider profile not found.");

            if (provider.UserId != userId) return Forbid();

            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                return BadRequest("Category does not exist.");

            if (!await _context.Regions.AnyAsync(r => r.id == dto.RegionId))
                return BadRequest("Region does not exist.");

            provider.BusinessName = dto.BusinessName;
            provider.Address = dto.Address;
            provider.RegionId = dto.RegionId;
            provider.CategoryId = dto.CategoryId;
            provider.Description = dto.Description;

            await _context.SaveChangesAsync();

            var result = await _context.ProviderProfiles
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(ToDetails)
                .FirstAsync();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var provider = await _context.ProviderProfiles.FirstOrDefaultAsync(p => p.Id == id);
            if (provider == null) return NotFound();

            if (provider.UserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            _context.ProviderProfiles.Remove(provider);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}