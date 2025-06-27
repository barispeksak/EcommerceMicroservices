using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.DTOs;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductItemController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm ürün öğelerini getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductItem>>> TumUrunOgeleriniGetir()
        {
            return await _context.ProductItems
                .Include(i => i.Product)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir ürün öğesini ID'ye göre getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductItem>> UrunOgesiGetir(int id)
        {
            var item = await _context.ProductItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return NotFound("Ürün öğesi bulunamadı.");

            return item;
        }

        /// <summary>
        /// Yeni bir ürün öğesi oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductItem>> UrunOgesiOlustur([FromBody] ProductItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = new ProductItem
            {
                SKU             = dto.Sku,
                QuantityInStock = dto.QuantityInStock,
                Price           = dto.Price,
                ProductImage    = dto.ProductImage
            };

            _context.ProductItems.Add(item);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, item);
        }

        /// <summary>
        /// Var olan bir ürün öğesini günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UrunOgesiGuncelle(int id, ProductItem item)
        {
            if (id != item.Id)
                return BadRequest("ID'ler uyuşmuyor.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (item.QuantityInStock <= 0)
                return BadRequest("Stok 0 veya negatif olamaz.");

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProductItems.Any(i => i.Id == id))
                    return NotFound("Ürün öğesi güncellenemedi çünkü mevcut değil.");
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Belirli bir ürün öğesini siler.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UrunOgesiSil(int id)
        {
            var item = await _context.ProductItems.FindAsync(id);
            if (item == null)
                return NotFound("Silinecek ürün öğesi bulunamadı.");

            _context.ProductItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}