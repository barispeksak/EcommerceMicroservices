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

        /// <summary>
        /// Tüm varyasyonları getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VariationDto>>> TumVaryasyonlariGetir()
        {
            var variations = await _variationService.GetAllVariationsAsync();
            return Ok(variations);
        }

        /// <summary>
        /// Belirli bir varyasyon ID'sine göre detay getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VariationDto>> VaryasyonGetir(int id)
        {
            var variation = await _variationService.GetVariationByIdAsync(id);
            
            if (variation == null)
                return NotFound("Varyasyon bulunamadı.");

            return Ok(variation);
        }

        /// <summary>
        /// Yeni bir varyasyon oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VariationDto>> VaryasyonOlustur([FromBody] CreateVariationDto dto)
        {
            var variation = await _variationService.CreateVariationAsync(dto);
            return CreatedAtAction(nameof(VaryasyonGetir), new { id = variation.Id }, variation);
        }

        /// <summary>
        /// Varyasyonu günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VaryasyonGuncelle(int id, [FromBody] UpdateVariationDto varyasyon)
        {
            if (id != varyasyon.Id)
                return BadRequest("ID'ler uyuşmuyor.");

            var result = await _variationService.UpdateVariationAsync(varyasyon);
            
            if (!result)
                return NotFound("Varyasyon güncellenemedi çünkü mevcut değil.");

            return NoContent();
        }

        /// <summary>
        /// Belirli bir varyasyonu siler.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VaryasyonSil(int id)
        {
            var result = await _variationService.DeleteVariationAsync(id);
            
            if (!result)
                return NotFound("Silinecek varyasyon bulunamadı.");

            return NoContent();
        }
    }
}