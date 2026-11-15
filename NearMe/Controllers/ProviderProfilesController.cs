using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NearMe.DTOs;
using NearMe.Services;
using NearMe.Services.Common;
using System.Security.Claims;

namespace NearMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProviderProfilesController : ControllerBase
    {
        private readonly ProviderService _service;

        public ProviderProfilesController(ProviderService service)
        {
            _service = service;
        }

        private string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return Ok(await _service.GetAllAsync(page, pageSize));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            return this.ToActionResult(await _service.GetByIdAsync(id));
        }

        [HttpGet("search")]
        public async Task<ActionResult> Search(
            [FromQuery] string? name,
            [FromQuery] int? categoryId,
            [FromQuery] int? regionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return Ok(await _service.SearchAsync(name, categoryId, regionId, page, pageSize));
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult> GetMyProfile()
        {
            if (CurrentUserId == null) return Unauthorized();
            return this.ToActionResult(await _service.GetMyProfileAsync(CurrentUserId));
        }

        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult> Create([FromBody] CreateProviderProfileDto dto)
        {
            if (CurrentUserId == null) return Unauthorized();

            var result = await _service.CreateAsync(CurrentUserId, dto);
            if (!result.Success) return this.ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult> Edit(int id, [FromBody] CreateProviderProfileDto dto)
        {
            if (CurrentUserId == null) return Unauthorized();
            return this.ToActionResult(await _service.UpdateAsync(id, CurrentUserId, dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == null) return Unauthorized();

            var result = await _service.DeleteAsync(id, CurrentUserId, User.IsInRole("Admin"));
            if (result.Success) return NoContent();

            return this.ToActionResult(result);
        }
    }
}
