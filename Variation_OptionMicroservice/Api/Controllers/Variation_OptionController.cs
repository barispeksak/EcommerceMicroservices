using Microsoft.AspNetCore.Mvc;
using Variation_OptionMicroservice.Service.DTOs;
using Variation_OptionMicroservice.Service.Interfaces;

namespace Variation_OptionMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Variation_OptionController : ControllerBase
    {
        private readonly IVariationOptionService _variationOptionService;

        public Variation_OptionController(IVariationOptionService variationOptionService)
        {
            _variationOptionService = variationOptionService;
        }

        /// <summary>
        /// Tüm varyasyon seçeneklerini getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VariationOptionDto>>> TumSecenekleriGetir()
        {
            var options = await _variationOptionService.GetAllOptionsAsync();
            return Ok(options);
        }

        /// <summary>
        /// Belirli bir ID'ye sahip varyasyon seçeneğini getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VariationOptionDto>> SecenekGetir(int id)
        {
            var option = await _variationOptionService.GetOptionByIdAsync(id);
            
            if (option == null)
                return NotFound("Varyasyon seçeneği bulunamadı.");

            return Ok(option);
        }

        /// <summary>
        /// Belirli bir varyasyona ait tüm seçenekleri getirir.
        /// </summary>
        [HttpGet("variation/{variationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VariationOptionDto>>> VaryasyonaGoreSecenekleriGetir(int variationId)
        {
            var options = await _variationOptionService.GetOptionsByVariationIdAsync(variationId);
            return Ok(options);
        }

        /// <summary>
        /// Yeni bir varyasyon seçeneği oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VariationOptionDto>> VaryasyonSecenegiOlustur([FromBody] CreateVariationOptionDto dto)
        {
            try
            {
                var option = await _variationOptionService.CreateOptionAsync(dto);
                return CreatedAtAction(nameof(SecenekGetir), new { id = option.Id }, option);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

                /// <summary>
        /// Varyasyon seçeneğini günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SecenekGuncelle(int id, [FromBody] UpdateVariationOptionDto secenek)
        {
            if (id != secenek.Id)
                return BadRequest("ID'ler uyuşmuyor.");

            try
            {
                var result = await _variationOptionService.UpdateOptionAsync(secenek);
                
                if (!result)
                    return NotFound("Varyasyon seçeneği güncellenemedi çünkü mevcut değil.");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir varyasyon seçeneğini siler.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SecenekSil(int id)
        {
            var result = await _variationOptionService.DeleteOptionAsync(id);
            
            if (!result)
                return NotFound("Silinecek varyasyon seçeneği bulunamadı.");

            return NoContent();
        }
    }
}