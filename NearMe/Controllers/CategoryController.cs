using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NearMe.DTOs;
using NearMe.Services;
using NearMe.Services.Common;

namespace NearMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;
        private readonly ImageStorageService _images;

        public CategoryController(CategoryService service, ImageStorageService images)
        {
            _service = service;
            _images = images;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            return this.ToActionResult(await _service.GetByIdAsync(id));
        }

        [HttpGet("{name}")]
        public async Task<ActionResult> Search(string name)
        {
            var category = await _service.GetByNameAsync(name);
            if (category == null) return NoContent();
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CategoryDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.Success) return this.ToActionResult(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] CategoryDto dto)
        {
            return this.ToActionResult(await _service.UpdateAsync(id, dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (result.Success) return NoContent();
            return this.ToActionResult(result);
        }

        [HttpPost("upload-icon")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UploadIcon(IFormFile file)
        {
            var result = await _images.SaveAsync(file, "categories");
            if (!result.Success) return this.ToActionResult(result);
            return Ok(new { iconPath = result.Data });
        }
    }
}
