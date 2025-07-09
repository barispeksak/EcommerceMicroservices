using Microsoft.AspNetCore.Mvc;
using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
using ShoppingCartMicroservice_Service.Models;
using ShoppingCartMicroservice_Service.Services;
using Swashbuckle.AspNetCore.Annotations;
using MongoDB.Bson;
using System.Security.Claims;
using System.Text.Json;

namespace ShoppingCartMicroservice_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService     _shoppingCartService;
    private readonly ShoppingCartActionLogger _logger;
    private readonly ProductItemClient        _itemClient;
    private readonly ProductClient            _productClient;

    public ShoppingCartController(
        IShoppingCartService     shoppingCartService,
        ShoppingCartActionLogger logger,
        ProductItemClient        itemClient,
        ProductClient            productClient)
    {
        _shoppingCartService = shoppingCartService;
        _logger              = logger;
        _itemClient          = itemClient;
        _productClient       = productClient;
    }

    /* ───────────────────────── Helpers ───────────────────────── */

    private static BsonDocument Wrap(object? o) =>
        o is null ? new BsonDocument { { "msg", "null" } }
                  : BsonDocument.Parse(JsonSerializer.Serialize(o));

    private string CorrelationId() =>
        HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? HttpContext.TraceIdentifier;

    private string PerformedBy() =>
        HttpContext.Request.Headers["X-User-Email"].FirstOrDefault()
        ?? "anonymous";

    /* ───────────────────────── Endpoints ─────────────────────── */

    [HttpPost("item")]
    [SwaggerOperation(Summary = "Sepete ürün ekle/güncelle")]
    public async Task<IActionResult> AddOrUpdateItem([FromBody] CreateShoppingCartDto dto)
    {
        var userId     = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var cid        = CorrelationId();
        var performed  = PerformedBy();

        /* 1️⃣ Miktar doğrulaması */
        if (dto.Quantity < 1)
        {
            const string msg = "Eklemek istediğiniz miktar 1'den küçük olamaz.";
            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action         = "AddOrUpdateItem",
                Level          = "Warn",
                Message        = msg,
                CorrelationId  = cid,
                Timestamp      = DateTime.UtcNow,
                PerformedByEmail = performed,
                CartId         = userId,
                ProductId      = dto.ProductItemId.ToString(),
                Quantity       = dto.Quantity,
                Description    = Wrap(dto)
            });
            return BadRequest(new { success = false, message = msg });
        }

        /* 2️⃣ ProductItem çek */
        var productItem = await _itemClient.GetByIdAsync(dto.ProductItemId);
        if (productItem is null)
        {
            const string msg = "ProductItem bulunamadı!";
            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action        = "AddOrUpdateItem",
                Level         = "Warn",
                Message       = msg,
                CorrelationId = cid,
                Timestamp     = DateTime.UtcNow,
                PerformedByEmail = performed,
                CartId        = userId,
                ProductId     = dto.ProductItemId.ToString(),
                Quantity      = dto.Quantity,
                Description   = Wrap(dto)
            });
            return BadRequest(new { success = false, message = msg });
        }

        /* 3️⃣ Ana Product çek */
        var product = await _productClient.GetByIdAsync(productItem.ProductId);
        if (product is null)
        {
            const string msg = "Product (ana ürün) bulunamadı!";
            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action        = "AddOrUpdateItem",
                Level         = "Warn",
                Message       = msg,
                CorrelationId = cid,
                Timestamp     = DateTime.UtcNow,
                PerformedByEmail = performed,
                CartId        = userId,
                ProductId     = dto.ProductItemId.ToString(),
                Quantity      = dto.Quantity,
                Description   = Wrap(new { Request = dto, ProductItem = productItem })
            });
            return BadRequest(new { success = false, message = msg });
        }

        /* 4️⃣ Stok kontrolü */
        if (dto.Quantity > productItem.QuantityInStock)
        {
            string msg = $"Yeterli stok yok! Mevcut stok: {productItem.QuantityInStock}";
            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action        = "AddOrUpdateItem",
                Level         = "Warn",
                Message       = msg,
                CorrelationId = cid,
                Timestamp     = DateTime.UtcNow,
                PerformedByEmail = performed,
                CartId        = userId,
                ProductId     = dto.ProductItemId.ToString(),
                Quantity      = dto.Quantity,
                Description   = Wrap(new { Request = dto, ProductItem = productItem })
            });
            return BadRequest(new { success = false, message = msg });
        }

        /* 5️⃣ Sepete ekle / güncelle */
        try
        {
            await _shoppingCartService.AddOrUpdateItemAsync(userId, dto);

            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action        = "AddOrUpdateItem",
                Level         = "Info",
                Message       = "Ürün başarıyla sepete eklendi/güncellendi.",
                CorrelationId = cid,
                Timestamp     = DateTime.UtcNow,
                PerformedByEmail = performed,
                CartId        = userId,
                ProductId     = dto.ProductItemId.ToString(),
                Quantity      = dto.Quantity,
                Description   = Wrap(new { Request = dto, ProductItem = productItem, Product = product })
            });

            return Ok(new { success = true, message = "Ürün başarıyla sepete eklendi!" });
        }
        catch (Exception ex)
        {
            const string msg = "Bilinmeyen hata oluştu!";
            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action        = "AddOrUpdateItem",
                Level         = "Error",
                Message       = ex.Message,
                CorrelationId = cid,
                Timestamp     = DateTime.UtcNow,
                PerformedByEmail = performed,
                CartId        = userId,
                ProductId     = dto.ProductItemId.ToString(),
                Quantity      = dto.Quantity,
                Description   = Wrap(new { Request = dto, Exception = ex.Message })
            });
            return StatusCode(500, new { success = false, message = msg });
        }
    }

    /* ───────── Delete item ───────── */

    [HttpDelete("item/{productItemId}")]
    [SwaggerOperation(Summary = "Sepetten ürün siler")]
    public async Task<IActionResult> RemoveItem(int productItemId)
    {
        var userId    = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var cid       = CorrelationId();
        var performed = PerformedBy();

        var cart = await _shoppingCartService.GetCartDetailsForUser(userId);
        if (cart.All(i => i.Id != productItemId))
        {
            const string msg = "Silmek istediğiniz ürün sepette bulunamadı.";
            await _logger.LogAsync(new ShoppingCartActionLog
            {
                Action        = "RemoveItem",
                Level         = "Warn",
                Message       = msg,
                CorrelationId = cid,
                Timestamp     = DateTime.UtcNow,
                PerformedByEmail = performed,
                CartId        = userId,
                ProductId     = productItemId.ToString(),
                Description   = Wrap(new { ProductItemId = productItemId })
            });
            return NotFound(new { success = false, message = msg });
        }

        await _shoppingCartService.RemoveItemAsync(userId, productItemId);

        await _logger.LogAsync(new ShoppingCartActionLog
        {
            Action        = "RemoveItem",
            Level         = "Info",
            Message       = "Ürün başarıyla sepetten silindi.",
            CorrelationId = cid,
            Timestamp     = DateTime.UtcNow,
            PerformedByEmail = performed,
            CartId        = userId,
            ProductId     = productItemId.ToString(),
            Description   = Wrap(new { ProductItemId = productItemId })
        });

        return Ok(new { success = true, message = "Ürün başarıyla sepetten silindi!" });
    }

    /* ───────── Clear cart ───────── */

    [HttpDelete("clear")]
    [SwaggerOperation(Summary = "Sepeti temizler")]
    public async Task<IActionResult> ClearCart()
    {
        var userId    = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var cid       = CorrelationId();
        var performed = PerformedBy();

        await _shoppingCartService.ClearAsync(userId);

        await _logger.LogAsync(new ShoppingCartActionLog
        {
            Action        = "ClearCart",
            Level         = "Info",
            Message       = "Sepet başarıyla temizlendi.",
            CorrelationId = cid,
            Timestamp     = DateTime.UtcNow,
            PerformedByEmail = performed,
            CartId        = userId,
            Description   = Wrap(new { UserId = userId })
        });

        return Ok(new { success = true, message = "Sepet başarıyla temizlendi!" });
    }

    /* ───────── Get details ───────── */

    [HttpGet("details")]
    [SwaggerOperation(Summary = "Kullanıcının sepet detayları")]
    public async Task<IActionResult> GetCartDetails()
    {
        var userId    = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var cid       = CorrelationId();
        var performed = PerformedBy();

        var details = await _shoppingCartService.GetCartDetailsForUser(userId);

        await _logger.LogAsync(new ShoppingCartActionLog
        {
            Action        = "GetCartDetails",
            Level         = "Info",
            Message       = "Kullanıcı sepet detayları listelendi.",
            CorrelationId = cid,
            Timestamp     = DateTime.UtcNow,
            PerformedByEmail = performed,
            CartId        = userId,
            Description   = Wrap(details)
        });

        return Ok(details);
    }
}
