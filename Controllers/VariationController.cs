using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VariationController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm varyasyonları getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Variation>>> TumVaryasyonlariGetir()
        {
            return await _context.Variations
                .Include(v => v.Category)
                .Include(v => v.Options)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir varyasyon ID’sine göre detay getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Variation>> VaryasyonGetir(int id)
        {
            var varyasyon = await _context.Variations
                .Include(v => v.Category)
                .Include(v => v.Options)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (varyasyon == null)
                return NotFound("Varyasyon bulunamadı.");

            return varyasyon;
        }

        /// <summary>
        /// Yeni bir varyasyon oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Variation>> VaryasyonOlustur(Variation varyasyon)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Variations.Add(varyasyon);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(VaryasyonGetir), new { id = varyasyon.Id }, varyasyon);
        }

        /// <summary>
        /// Varyasyonu günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VaryasyonGuncelle(int id, Variation varyasyon)
        {
            if (id != varyasyon.Id)
                return BadRequest("ID'ler uyuşmuyor.");

            _context.Entry(varyasyon).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Variations.Any(v => v.Id == id))
                    return NotFound("Varyasyon güncellenemedi çünkü mevcut değil.");
                else
                    throw;
            }

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
            var varyasyon = await _context.Variations.FindAsync(id);
            if (varyasyon == null)
                return NotFound("Silinecek varyasyon bulunamadı.");

            _context.Variations.Remove(varyasyon);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}