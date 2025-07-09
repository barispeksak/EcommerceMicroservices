using ProductMicroservice_Service.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ProductMicroservice_Service.DTOs;
using ProductMicroservice_Data.Models;
using ProductMicroservice_Service.Interfaces;
using System.Text.Json;

namespace ProductMicroservice_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ProductActionLogger _logger;

        public ProductsController(IProductService service, ProductActionLogger logger)
        {
            _service = service;
            _logger = logger;
        }

        private static BsonDocument WrapObject(object? obj) =>
            obj is null ? new BsonDocument { { "msg", "null" } } : BsonDocument.Parse(JsonSerializer.Serialize(obj));

        private string GetCorrelationId() =>
            HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

        private string GetPerformedByEmail() =>
            HttpContext.Request.Headers["X-User-Email"].FirstOrDefault() ?? "anonymous";

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();
            var products = await _service.GetAllAsync();

            await _logger.LogAsync(new ProductActionLog
            {
                Action = "GetAll",
                Level = "Info",
                Message = "Tüm ürünler listelendi.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                Description = WrapObject(products)
            });

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            try
            {
                var product = await _service.GetAsync(id);
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "GetById",
                    Level = "Info",
                    Message = "Ürün getirildi.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    ProductCategoryId = product.CategoryId.ToString(),
                    Name = product.Name,
                    Image = product.Image,
                    Brand = product.Brand,
                    Description = WrapObject(product)
                });
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                string msg = "Girilen ID ile ürün bulunamadı.";
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "GetById",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            var categoryExists = await _service.CategoryExistsAsync(dto.CategoryId);
            if (!categoryExists)
            {
                string msg = $"Kategori ID {dto.CategoryId} bulunamadı.";
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Create",
                    Level = "Error",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    ProductCategoryId = dto.CategoryId.ToString(),
                    Name = dto.Name,
                    Image = dto.Image,
                    Brand = dto.Brand,
                    Description = WrapObject(dto)
                });

                return BadRequest(new { message = msg });
            }

            try
            {
                var created = await _service.CreateAsync(dto);
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Create",
                    Level = "Info",
                    Message = "Ürün başarıyla oluşturuldu.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    ProductCategoryId = created.CategoryId.ToString(),
                    Name = created.Name,
                    Image = created.Image,
                    Brand = created.Brand,
                    Description = WrapObject(created)
                });
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                string msg = "Ürün oluşturulamadı. Sunucu hatası.";
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Create",
                    Level = "Error",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    Description = WrapObject(new { Request = dto, Exception = ex.Message })
                });
                return StatusCode(500, new { message = msg });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            // Eğer CategoryId doluysa kontrol et
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _service.CategoryExistsAsync(dto.CategoryId.Value);
                if (!categoryExists)
                {
                    string msg = $"Kategori ID {dto.CategoryId.Value} bulunamadı.";
                    await _logger.LogAsync(new ProductActionLog
                    {
                        Action = "Update",
                        Level = "Fail",
                        Message = msg,
                        CorrelationId = cid,
                        Timestamp = DateTime.UtcNow,
                        PerformedByEmail = performedBy,
                        ProductCategoryId = dto.CategoryId.Value.ToString(),
                        Name = dto.Name,
                        Image = dto.Image,
                        Brand = dto.Brand,
                        Description = WrapObject(dto)
                    });

                    return BadRequest(new { message = msg });
                }
            }

            try
            {
                var updated = await _service.UpdateAsync(id, dto);

                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Update",
                    Level = "Info",
                    Message = "Ürün başarıyla güncellendi.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    ProductCategoryId = updated.CategoryId.ToString(),
                    Name = updated.Name,
                    Image = updated.Image,
                    Brand = updated.Brand,
                    Description = WrapObject(updated)
                });

                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                string msg = "Güncellenecek ürün bulunamadı.";
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Update",
                    Level = "Fail",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }
            catch (Exception ex)
            {
                string msg = "Ürün güncellenemedi. Sunucu hatası.";
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Update",
                    Level = "Fail",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    Description = WrapObject(new { Request = dto, Exception = ex.Message })
                });
                return StatusCode(500, new { message = msg });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            try
            {
                var existing = await _service.GetAsync(id);
                await _service.DeleteAsync(id);

                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Delete",
                    Level = "Info",
                    Message = "Ürün başarıyla silindi.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    ProductCategoryId = existing.CategoryId.ToString(),
                    Name = existing.Name,
                    Image = existing.Image,
                    Brand = existing.Brand,
                    Description = WrapObject(existing)
                });

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                string msg = "Silinecek ürün bulunamadı.";
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Delete",
                    Level = "Fail",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }
            catch (Exception ex)
            {
                string msg = "Ürün silinemedi. Sunucu hatası.";
                await _logger.LogAsync(new ProductActionLog
                {
                    Action = "Delete",
                    Level = "Fail",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    Description = WrapObject(new { Id = id, Exception = ex.Message })
                });
                return StatusCode(500, new { message = msg });
            }
        }
    }
}
