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
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _service;

        public ReviewController(ReviewService service)
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

        [HttpGet("provider/{id:int}")]
        public async Task<ActionResult> GetByProvider(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return this.ToActionResult(await _service.GetByProviderAsync(id, page, pageSize));
        }

        [HttpGet("provider/{providerId:int}/mine")]
        [Authorize]
        public async Task<ActionResult> GetMine(int providerId)
        {
            if (CurrentUserId == null) return Unauthorized();
            return this.ToActionResult(await _service.GetMineAsync(CurrentUserId, providerId));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateReviewDto dto)
        {
            if (CurrentUserId == null) return Unauthorized();

            var result = await _service.CreateAsync(CurrentUserId, dto);
            if (!result.Success) return this.ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateReviewDto dto)
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