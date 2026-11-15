using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NearMe.DTOs;
using NearMe.Services;
using NearMe.Services.Common;

namespace NearMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegionController : ControllerBase
    {
        private readonly RegionService _service;

        public RegionController(RegionService service)
        {
            _service = service;
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
            var region = await _service.GetByNameAsync(name);
            if (region == null) return NoContent();
            return Ok(region);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] RegionDto dto)
        {
            var region = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = region.id }, region);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] RegionDto dto)
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
    }
}
