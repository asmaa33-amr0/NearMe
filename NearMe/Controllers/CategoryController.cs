using Microsoft.AspNetCore.Mvc;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;

namespace NearMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public ActionResult GetAll()
        {
            var Category = _context.Categories.ToList();
            return Ok(Category);

        }
        [HttpGet("{id:int}")]
        public ActionResult GetById(int id)
        {
            var Category = _context.Categories.FirstOrDefault(c => c.Id == id);
            return Ok(Category);
        }

        [HttpPost]
        public ActionResult Create([FromBody] CategoryDto category)
        {
            var categoryi = new Category
            {
                Name = category.Name,
                IconPath = category.IconPath

            };
            _context.Categories.Add(categoryi);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = categoryi.Id }, categoryi);
        }
        [HttpPut("{id:int}")]
        public ActionResult Update(int id,[FromBody] CategoryDto category) {
        
        var categoryi = _context.Categories.FirstOrDefault(c=>c.Id == id);
            categoryi.Name = category.Name;
            categoryi.IconPath = category.IconPath;
            _context.SaveChanges();
            return Ok(categoryi);
        }

        [HttpDelete]
        public ActionResult DeleteById(int id) { 

            var category = _context.Categories.FirstOrDefault(c=>c.Id == id);
            _context.Categories.Remove(category);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpGet("{name}")]
        public ActionResult search(string name)
        {
            var category = _context.Categories.FirstOrDefault(category => category.Name == name);
            if (category == null)
            {
                return NoContent();
            }
            return Ok(category);
        }
    }
}
