using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [Authorize]
        public ActionResult Create([FromBody] providerImageDTO providerImage)
        {
            if (providerImage == null || string.IsNullOrEmpty(providerImage.imgurl))
            {
                return BadRequest("Invalid image data.");
            }

            // Get the provider ID from the authenticated user's claims
            var providerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(providerIdClaim) || !int.TryParse(providerIdClaim, out int providerProfileId))
            {
                return Unauthorized("Unable to identify the provider.");
            }

            // Map DTO to the entity and link it to the provider
            var image = new ProviderImage
            {
                ImageUrl = providerImage.imgurl,
                ProviderProfileId = providerProfileId
            };

            _context.ProviderImages.Add(image);
            _context.SaveChanges(); // Synchronous save

            return CreatedAtAction(nameof(GetById), new { id = image.Id }, image);
        }

        [HttpPut("{id:int}")]
        public ActionResult Update(  int id, [FromBody] providerImageDTO providerImage) { 
        
        var image =  _context.ProviderImages.Find(id);
            image.ImageUrl = providerImage.imgurl;
            _context.SaveChanges();

            return Ok(image);
        
        }
        [HttpDelete]
        public ActionResult DeleteById(int id) {
            var img = _context.ProviderImages.FirstOrDefault(img => img.Id == id);
            _context.ProviderImages.Remove (img);
            _context.SaveChanges();
            return NoContent(); 
        }

    }
}
