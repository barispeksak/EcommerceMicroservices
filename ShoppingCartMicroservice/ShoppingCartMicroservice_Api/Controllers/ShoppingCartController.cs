using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using MongoDB.Bson;
using ShoppingCartMicroservice_Service.Models;
using ShoppingCartMicroservice_Service.Services; // Logger burada
using System.Text.Json;


namespace ShoppingCartMicroservice_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ShoppingCartActionLogger _logger;
        private readonly ProductClient _productClient;

        public ShoppingCartController(
            IShoppingCartService shoppingCartService,
            ShoppingCartActionLogger logger,
            ProductClient productClient)
        {
            _shoppingCartService = shoppingCartService;
            _logger = logger;
            _productClient = productClient;
        }

        // Yardımcılar
        private static BsonDocument WrapObject(object? obj) =>
            obj is null
                ? new BsonDocument { { "msg", "null" } }
                : BsonDocument.Parse(JsonSerializer.Serialize(obj));

        private string GetCorrelationId() =>
            HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

        private string GetPerformedByEmail() =>
            HttpContext.Request.Headers["X-User-Email"].FirstOrDefault() ?? "anonymous";

        [HttpPost("item")]
        [SwaggerOperation(Summary = "Sepete ürün ekle/güncelle", Description = "Kullanıcı kendi sepetine ürün ekler veya miktar günceller.")]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] CreateShoppingCartDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            // 1️⃣ Miktar hatası
            if (dto.Quantity < 1)
            {
                string msg = "Eklemek istediğiniz miktar 1'den küçük olamaz.";
                await _logger.LogAsync(new ShoppingCartActionLog
                {
                    Action = "AddOrUpdateItem",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    CartId = userId,
                    ProductId = dto.ProductItemId.ToString(),
                    Quantity = dto.Quantity,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { success = false, message = msg });
            }

            // 2️⃣ ProductItem çek
            var productItem = await _productClient.GetProductItemByIdAsync(dto.ProductItemId);
            if (productItem == null)
            {
                string msg = "ProductItem bulunamadı!";
                await _logger.LogAsync(new ShoppingCartActionLog
                {
                    Action = "AddOrUpdateItem",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    CartId = userId,
                    ProductId = dto.ProductItemId.ToString(),
                    Quantity = dto.Quantity,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { success = false, message = msg });
            }

            // 3️⃣ Ana Product çek
            var product = await _productClient.GetProductByIdAsync(productItem.ProductId);
            if (product == null)
            {
                string msg = "Product (ana ürün) bulunamadı!";
                await _logger.LogAsync(new ShoppingCartActionLog
                {
                    Action = "AddOrUpdateItem",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    CartId = userId,
                    ProductId = dto.ProductItemId.ToString(),
                    Quantity = dto.Quantity,
                    Description = WrapObject(new { Request = dto, ProductItem = productItem })
                });
                return BadRequest(new { success = false, message = msg });
            }

            // 4️⃣ Stok kontrolü
            if (dto.Quantity > productItem.QuantityInStock)
            {
                string msg = $"Yeterli stok yok! Mevcut stok: {productItem.QuantityInStock}";
                await _logger.LogAsync(new ShoppingCartActionLog
                {
                    Action = "AddOrUpdateItem",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    CartId = userId,
                    ProductId = dto.ProductItemId.ToString(),
                    Quantity = dto.Quantity,
                    Description = WrapObject(new { Request = dto, ProductItem = productItem })
                });
                return BadRequest(new { success = false, message = msg });
            }

            // 5️⃣ Başarılı ekleme/güncelleme
            try
            {
                await _shoppingCartService.AddOrUpdateItemAsync(userId, dto);

                await _logger.LogAsync(new ShoppingCartActionLog
                {
                    Action = "AddOrUpdateItem",
                    Level = "Info",
                    Message = "Ürün başarıyla sepete eklendi/güncellendi.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    CartId = userId,
                    ProductId = dto.ProductItemId.ToString(),
                    Quantity = dto.Quantity,
                    Description = WrapObject(new { Request = dto, ProductItem = productItem, Product = product })
                });

                return Ok(new { success = true, message = "Ürün başarıyla sepete eklendi!" });
            }
            catch (Exception ex)
            {
                string msg = "Bilinmeyen hata oluştu!";
                await _logger.LogAsync(new ShoppingCartActionLog
                {
                    Action = "AddOrUpdateItem",
                    Level = "Error",
                    Message = ex.Message,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    CartId = userId,
                    ProductId = dto.ProductItemId.ToString(),
                    Quantity = dto.Quantity,
                    Description = WrapObject(new { Request = dto, Exception = ex.Message })
                });

                return StatusCode(500, new { success = false, message = msg });
            }
        }

        [HttpDelete("item/{productItemId}")]
        [SwaggerOperation(Summary = "Sepetten ürün siler", Description = "Kullanıcı kendi sepetinden bir ürünü siler.")]
        public async Task<IActionResult> RemoveItem(int productItemId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            // Sepette var mı kontrolü
            var cart = await _shoppingCartService.GetCartDetailsForUser(userId);
            var exists = cart.Any(i => i.Id == productItemId);
            if (!exists)
            {
                string msg = "Silmek istediğiniz ürün sepette bulunamadı.";
                await _logger.LogAsync(new ShoppingCartActionLog
                {
                    Action = "RemoveItem",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    CartId = userId,
                    ProductId = productItemId.ToString(),
                    Description = WrapObject(new { ProductItemId = productItemId })
                });
                return NotFound(new { success = false, message = msg });
            }

            await _shoppingCartService.RemoveItemAsync(userId, productItemId);

            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action = "RemoveItem",
                Level = "Info",
                Message = "Ürün başarıyla sepetten silindi.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                CartId = userId,
                ProductId = productItemId.ToString(),
                Description = WrapObject(new { ProductItemId = productItemId })
            });

            return Ok(new { success = true, message = "Ürün başarıyla sepetten silindi!" });
        }

        [HttpDelete("clear")]
        [SwaggerOperation(Summary = "Sepeti temizler", Description = "Kullanıcı kendi sepetini tamamen temizler.")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            await _shoppingCartService.ClearAsync(userId);

            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action = "ClearCart",
                Level = "Info",
                Message = "Sepet başarıyla temizlendi.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                CartId = userId,
                Description = WrapObject(new { UserId = userId })
            });

            return Ok(new { success = true, message = "Sepet başarıyla temizlendi!" });
        }

        [HttpGet("details")]
        [SwaggerOperation(Summary = "Kullanıcıya ait detaylı sepet bilgisi", Description = "Kullanıcının kendi sepetini, ürünlerin tam detaylarıyla döner.")]
        public async Task<IActionResult> GetCartDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            var details = await _shoppingCartService.GetCartDetailsForUser(userId);

            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action = "GetCartDetails",
                Level = "Info",
                Message = "Kullanıcı sepet detayları listelendi.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                CartId = userId,
                Description = WrapObject(details)
            });

            return Ok(details);
        }
    }
}
