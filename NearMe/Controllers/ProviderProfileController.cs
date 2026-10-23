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
    public class ProviderProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProviderProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public ActionResult<IEnumerable<ProviderListItemDto >> GetAll()
        {
            var providerProfiles = _context.ProviderProfiles
                .Select(p => new ProviderListItemDto
                {
                    Id = p.Id,
                    BusinessName = p.BusinessName,
                    Address = p.Address,
                    CategoryName = p.Category.Name,  
                    RegionName = p.Region.name   
                    
                })
                .ToList();  

            return Ok(providerProfiles);
        }

        
        [HttpGet("{id:int}")]
        public ActionResult<ProviderResponseDto> GetById(int id)
        {
            var provider = _context.ProviderProfiles
                .Where(p => p.Id == id)  
                .Select(p => new ProviderResponseDto
                {
                    Id = p.Id,
                    BusinessName = p.BusinessName,
                    Address = p.Address,
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = p.Reviews.Count(),
                    Description = p.Description,

                    category = new CategoryDto
                    {
                        Name = p.Category.Name,
                        IconPath = p.Category.IconPath
                    },

                  
                    Region =  p.Region.name
                    ,

                    Gallery = p.Gallery.Select(img => new providerImageDTO
                    {
                        imgurl = img.ImageUrl
                    }).ToList()
                })
                .FirstOrDefault(); 

            if (provider == null)
            {
                return NotFound(); 
            }

            return Ok(provider);
        }
        
        [HttpGet("search")]
        public ActionResult<IEnumerable<ProviderListItemDto>> Search(
    [FromQuery] string? name,
    [FromQuery] int? categoryId,
    [FromQuery] int? regionId,
    [FromQuery] string? BusinessName)
        {
           var query = _context.ProviderProfiles.AsQueryable();
             
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.BusinessName.Contains(name));
            }

            
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

              if (regionId.HasValue)
            {
                query = query.Where(p => p.RegionId == regionId.Value);
            }

             var result = query
                .Select(p => new ProviderListItemDto
                {
                    Id = p.Id,
                    BusinessName = p.BusinessName,
                    Address = p.Address,
                    CategoryName = p.Category.Name,
                    RegionName = p.Region.name,
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = p.Reviews.Count()

                })
                .ToList();

            return Ok(result);
        }
        [HttpGet("my-profile")]
        [Authorize]
        public ActionResult<ProviderResponseDto> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var provider = _context.ProviderProfiles
                .Where(p => p.UserId == userId)
                .Select(p => new ProviderResponseDto
                {
                    Id = p.Id,
                    BusinessName = p.BusinessName,
                    Address = p.Address,
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = p.Reviews.Count(),
                    Description = p.Description,
                    category = new CategoryDto { Name = p.Category.Name, IconPath = p.Category.IconPath },
                    Region = p.Region.name,
                    Gallery = p.Gallery.Select(img => new providerImageDTO { imgurl = img.ImageUrl }).ToList()
                })
                .FirstOrDefault();

            if (provider == null) return NotFound("You don't have a provider profile yet.");

            return Ok(provider);
        }
        [HttpPost]
        [Authorize] 
        public ActionResult Create([FromBody] CreateProviderProfileDto providerDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var provider = new ProviderProfile
            {
                UserId = userId,  
                BusinessName = providerDTO.BusinessName,
                Address = providerDTO.Address,
                RegionId = providerDTO.RegionId,
                CategoryId = providerDTO.CategoryId,
                Description = providerDTO.Description,
                IsApproved = false
            };

            _context.ProviderProfiles.Add(provider);
            _context.SaveChanges();
            return Ok(provider);
        }
        [HttpPut("{id:int}")]
        [Authorize]
        public ActionResult Edit(int id, [FromBody] CreateProviderProfileDto providerDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var provider = _context.ProviderProfiles.FirstOrDefault(p => p.Id == id);
            if (provider == null) return NotFound();
              if (provider.UserId != userId)
            {
                return Forbid(); 
            }

            provider.Address = providerDTO.Address;
            provider.RegionId = providerDTO.RegionId;
            provider.CategoryId = providerDTO.CategoryId;
            provider.Description = providerDTO.Description;
            provider.BusinessName = providerDTO.BusinessName;

            _context.SaveChanges();
            return Ok(provider);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var provider = _context.ProviderProfiles.FirstOrDefault(p => p.Id == id);
            _context.ProviderProfiles.Remove(provider);
            _context.SaveChanges();
            return Ok();
        }
    }
}
