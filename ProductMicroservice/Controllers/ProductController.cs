using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.DTOs;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm ürünleri getirir.
        /// </summary>
        /// <returns>Ürün listesi</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir ürün bilgisini getirir.
        /// </summary>
        /// <param name="id">Ürün ID</param>
        /// <returns>Ürün bilgisi</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Ürün bulunamadı.");

            return product;
        }

        /// <summary>
        /// Yeni ürün oluşturur.
        /// </summary>
        /// <param name="product">Ürün nesnesi</param>
        /// <returns>Oluşturulan ürün bilgisi</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    message = "Model doğrulama hatası",
                    errors = ModelState
                });

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Brand = dto.Brand,
                Image = dto.Image,
                CategoryId = dto.CategoryId,
                Items = dto.Items.Select(i => new ProductItem
                {
                    SKU = i.Sku,
                    QuantityInStock = i.QuantityInStock,
                    Price = i.Price,
                    ProductImage = i.ProductImage
                }).ToList()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, product);

        }


        /// <summary>
        /// Ürün günceller.
        /// </summary>
        /// <param name="id">Ürün ID</param>
        /// <param name="product">Güncellenmiş ürün nesnesi</param>
        /// <returns>HTTP sonucu</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest("Gönderilen ID ile ürün ID uyuşmuyor.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(p => p.Id == id))
                    return NotFound("Ürün güncellenemedi çünkü mevcut değil.");
                else
                    throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Ürün siler.
        /// </summary>
        /// <param name="id">Ürün ID</param>
        /// <returns>HTTP sonucu</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Silinecek ürün bulunamadı.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
