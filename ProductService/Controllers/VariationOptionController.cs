using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariationOptionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VariationOptionController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm varyasyon seçeneklerini getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VariationOption>>> TumSecenekleriGetir()
        {
            return await _context.VariationOptions
                .Include(vo => vo.Variation)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir ID'ye sahip varyasyon seçeneğini getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VariationOption>> SecenekGetir(int id)
        {
            var secenek = await _context.VariationOptions
                .Include(vo => vo.Variation)
                .FirstOrDefaultAsync(vo => vo.Id == id);

            if (secenek == null)
                return NotFound("Varyasyon seçeneği bulunamadı.");

            return secenek;
        }

        /// <summary>
        /// Yeni bir varyasyon seçeneği oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VariationOption>> SecenekOlustur(VariationOption secenek)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.VariationOptions.Add(secenek);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(SecenekGetir), new { id = secenek.Id }, secenek);
        }

        /// <summary>
        /// Varyasyon seçeneğini günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SecenekGuncelle(int id, VariationOption secenek)
        {
            if (id != secenek.Id)
                return BadRequest("ID'ler uyuşmuyor.");

            _context.Entry(secenek).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.VariationOptions.Any(vo => vo.Id == id))
                    return NotFound("Varyasyon seçeneği güncellenemedi çünkü mevcut değil.");
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Belirli bir varyasyon seçeneğini siler.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SecenekSil(int id)
        {
            var secenek = await _context.VariationOptions.FindAsync(id);
            if (secenek == null)
                return NotFound("Silinecek varyasyon seçeneği bulunamadı.");

            _context.VariationOptions.Remove(secenek);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
