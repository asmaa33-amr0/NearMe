using Microsoft.AspNetCore.Mvc;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
namespace NearMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class RegionController : ControllerBase
    {


        private readonly ApplicationDbContext _context;

        public RegionController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        [HttpGet]
        public IActionResult GetAll()
        {
            var regions = _context.Regions.ToList();
            return Ok(regions);

        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var region = _context.Regions.Find( id);
            return Ok(region);

        }

        [HttpPost]
        public IActionResult Create ([FromBody] RegionDto region)
        {
            var regionn = new Region
            {
                name = region.RegionName
            };
            _context.Regions.Add(regionn);
            
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = regionn.id}, regionn);
}
        [HttpPut("{id:int}")]
        public IActionResult updateregion (int id, [FromBody] RegionDto updatedRegion)
        {
            var region = _context.Regions.Find(id);
            region.name = updatedRegion.RegionName;
            _context.SaveChanges();
            return Ok(region);
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteById(int id)
        {
            var region = _context.Regions.Find(id);
            _context.Regions.Remove(region);
            _context.SaveChanges();
            return NoContent();
        }


        [HttpGet("{name}")]
        public ActionResult search(string name)
        {
            var region = _context.Regions.FirstOrDefault(region => region.name == name);
            if (region == null)
            {
                return NoContent();
            }
            return Ok(region);
        }
    }
}
