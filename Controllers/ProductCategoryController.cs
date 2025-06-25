using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductCategoryController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm ürün kategorilerini getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> TumKategorileriGetir()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir kategori ID’sine göre kategoriyi getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductCategory>> KategoriGetir(int id)
        {
            var kategori = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (kategori == null)
                return NotFound("Kategori bulunamadı.");

            return kategori;
        }

        /// <summary>
        /// Yeni bir ürün kategorisi oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductCategory>> KategoriOlustur(ProductCategory kategori)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Categories.Add(kategori);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(KategoriGetir), new { id = kategori.Id }, kategori);
        }

        /// <summary>
        /// Mevcut bir kategori bilgisini günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> KategoriGuncelle(int id, ProductCategory kategori)
        {
            if (id != kategori.Id)
                return BadRequest("Gönderilen ID ile kategori ID uyuşmuyor.");

            _context.Entry(kategori).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.Id == id))
                    return NotFound("Kategori güncellenemedi çünkü mevcut değil.");
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Belirli bir kategori ID’sine göre kategoriyi siler.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> KategoriSil(int id)
        {
            var kategori = await _context.Categories.FindAsync(id);
            if (kategori == null)
                return NotFound("Silinecek kategori bulunamadı.");

            _context.Categories.Remove(kategori);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}