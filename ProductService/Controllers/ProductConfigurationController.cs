using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductConfigurationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductConfigurationController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm ürün yapılandırmalarını getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductConfiguration>>> GetAllConfigurations()
        {
            return await _context.ProductConfigurations
                .Include(pc => pc.ProductItem)
                .Include(pc => pc.VariationOption)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir ürün yapılandırmasını getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductConfiguration>> GetConfiguration(int id)
        {
            var config = await _context.ProductConfigurations
                .Include(pc => pc.ProductItem)
                .Include(pc => pc.VariationOption)
                .FirstOrDefaultAsync(pc => pc.Id == id);

            if (config == null)
                return NotFound("Ürün yapılandırması bulunamadı.");

            return config;
        }

        /// <summary>
        /// Yeni bir ürün yapılandırması oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductConfiguration>> CreateConfiguration(ProductConfiguration config)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.ProductConfigurations.Add(config);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConfiguration), new { id = config.Id }, config);
        }

        /// <summary>
        /// Var olan bir ürün yapılandırmasını günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateConfiguration(int id, ProductConfiguration config)
        {
            if (id != config.Id)
                return BadRequest("Gönderilen ID ile yapılandırma ID uyuşmuyor.");

            _context.Entry(config).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProductConfigurations.Any(pc => pc.Id == id))
                    return NotFound("Ürün yapılandırması güncellenemedi çünkü mevcut değil.");
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Belirli bir ürün yapılandırmasını siler.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteConfiguration(int id)
        {
            var config = await _context.ProductConfigurations.FindAsync(id);
            if (config == null)
                return NotFound("Silinecek ürün yapılandırması bulunamadı.");

            _context.ProductConfigurations.Remove(config);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
