using Microsoft.AspNetCore.Mvc;
using VariationMicroservice.Service.DTOs;
using VariationMicroservice.Service.Interfaces;

namespace VariationMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariationController : ControllerBase
    {
        private readonly IVariationService _variationService;

        public VariationController(IVariationService variationService)
        {
            _variationService = variationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VariationDto>>> GetAll()
        {
            var variations = await _variationService.GetAllAsync();
            return Ok(variations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VariationDto>> GetById(int id)
        {
            try
            {
                var variation = await _variationService.GetAsync(id);
                return Ok(variation);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<VariationDto>> Create([FromBody] CreateVariationDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var variation = await _variationService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = variation.Id }, variation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVariationDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _variationService.UpdateAsync(id, updateDto);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _variationService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
