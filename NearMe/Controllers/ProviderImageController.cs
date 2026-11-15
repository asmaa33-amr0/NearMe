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
    public class ProviderImageController : ControllerBase
    {
        private readonly ProviderImageService _service;

        public ProviderImageController(ProviderImageService service)
        {
            _service = service;
        }

        private string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            return this.ToActionResult(await _service.GetByIdAsync(id));
        }

        [HttpGet("provider/{providerId:int}")]
        public async Task<ActionResult> GetByProviderId(int providerId)
        {
            return this.ToActionResult(await _service.GetByProviderAsync(providerId));
        }

        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult> Create([FromBody] ProviderImageDTO dto)
        {
            if (CurrentUserId == null) return Unauthorized();

            var result = await _service.AddAsync(CurrentUserId, dto);
            if (!result.Success) return this.ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult> Upload(IFormFile file)
        {
            if (CurrentUserId == null) return Unauthorized();

            var result = await _service.UploadAsync(CurrentUserId, file);
            if (!result.Success) return this.ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult> Update(int id, [FromBody] ProviderImageDTO dto)
        {
            if (CurrentUserId == null) return Unauthorized();
            return this.ToActionResult(await _service.UpdateAsync(id, CurrentUserId, dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Provider,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == null) return Unauthorized();

            var result = await _service.DeleteAsync(id, CurrentUserId, User.IsInRole("Admin"));
            if (result.Success) return NoContent();

            return this.ToActionResult(result);
        }
    }
}
