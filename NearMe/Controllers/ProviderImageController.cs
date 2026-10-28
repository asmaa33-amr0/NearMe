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
    public class ProviderImageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;



        public ProviderImageController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id:int}")]
        public ActionResult GetById(int id) { 
        var img= _context.ProviderImages. FirstOrDefault(img => img.Id == id);

            return Ok (img);
        }
        [HttpGet("provider/{providerId:int}")]
        public IActionResult GetByProviderId(int providerId)
        {
            var images = _context.ProviderImages.Where(img => img.ProviderProfileId == providerId).ToList();
            return Ok(images);
        }


        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult> Create([FromBody] ProviderImageDTO providerImage)
        {
            if (providerImage == null ||
                string.IsNullOrWhiteSpace(providerImage.ImageUrl))
            {
                return BadRequest("Invalid image data.");
            }

            // Get the authenticated user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Unable to identify the user.");
            }

            // Find the provider profile belonging to this user
            var providerProfile = await _context.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (providerProfile == null)
            {
                return NotFound("Provider profile not found.");
            }

            // Create the image and link it to the provider profile
            var image = new ProviderImage
            {
                ImageUrl = providerImage.ImageUrl,
                ProviderProfileId = providerProfile.Id
            };

            _context.ProviderImages.Add(image);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = image.Id },
                image
            );
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Provider)]
        public ActionResult Update(int id, [FromBody] ProviderImageDTO providerImage)
        {
            var image = _context.ProviderImages
                .Include(i => i.ProviderProfile)
                .FirstOrDefault(i => i.Id == id);
            if (image == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (image.ProviderProfile.UserId != userId) return Forbid();

            image.ImageUrl = providerImage.ImageUrl;
            _context.SaveChanges();
            return Ok(image);
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Provider")]
        public ActionResult DeleteById(int id)
        {
            var img = _context.ProviderImages.Include(i => i.ProviderProfile).FirstOrDefault(i => i.Id == id);
            if (img == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (img.ProviderProfile.UserId != userId)
            {
                return Forbid();
            }

            _context.ProviderImages.Remove(img);
            _context.SaveChanges();
            return NoContent();
        }

    }
}
