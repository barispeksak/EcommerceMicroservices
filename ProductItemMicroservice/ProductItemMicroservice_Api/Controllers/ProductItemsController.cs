using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ProductItemMicroservice_Service.DTOs;
using ProductItemMicroservice_Service.Interfaces;
using ProductItemMicroservice_Service.Services;
using ProductItemMicroservice_Data.Models; 
using MongoDB.Bson;
using System.Text.Json;

namespace ProductItemMicroservice_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductItemsController : ControllerBase
{
    private readonly IProductItemService _service;
    private readonly ProductApiClient _productApiClient;
    private readonly ProductItemActionLogger _logger;

    public ProductItemsController(
        IProductItemService service,
        ProductApiClient productApiClient,
        ProductItemActionLogger logger)
    {
        _service = service;
        _productApiClient = productApiClient;
        _logger = logger;
    }

    // Helper (Log Description için)
    private static BsonDocument WrapObject(object? obj) =>
        obj is null
            ? new BsonDocument { { "msg", "null" } }
            : BsonDocument.Parse(JsonSerializer.Serialize(obj));

    /* ---------- GET /api/productitems/{id} ---------- */
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Tek ürün stoğunu getir")]
    [ProducesResponseType(typeof(ProductItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductItemDto>> Get(int id)
    {
        var dto = await _service.GetByIdAsync(id);

        await _logger.LogAsync(new ProductItemActionLog
        {
            Action = "Get",
            Level = dto != null ? "Info" : "Warn",
            Message = dto != null ? "Ürün stoğu bulundu." : "Ürün stoğu bulunamadı.",
            Timestamp = DateTime.UtcNow,
            ProductId = dto?.ProductId.ToString(),
            Sku = dto?.Sku,
            QuantityInStock = dto?.QuantityInStock.ToString(),
            Description = WrapObject(dto)
        });

        return dto is null ? NotFound() : Ok(dto);
    }

    /* ---------- GET /api/productitems ---------- */
    [HttpGet]
    [SwaggerOperation(Summary = "Tüm ürün stoklarını getir")]
    [ProducesResponseType(typeof(IEnumerable<ProductItemDto>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<ProductItemDto>> GetAll()
    {
        var list = await _service.GetAllAsync();
        await _logger.LogAsync(new ProductItemActionLog
        {
            Action = "GetAll",
            Level = "Info",
            Message = $"Tüm ürün stokları listelendi. Count: {list.Count()}",
            Timestamp = DateTime.UtcNow,
            Description = WrapObject(list)
        });
        return list;
    }

    /* ---------- POST /api/productitems ---------- */
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni ürün stoğu ekle")]
    [ProducesResponseType(typeof(ProductItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductItemDto>> Post(CreateProductItemDto dto)
    {
        // ProductId kontrolü: Diğer mikroservisten çekiliyor
        bool productExists = await _productApiClient.ProductExists(dto.ProductId);

        if (!productExists)
        {
            string msg = "Bağlı olduğu ürün bulunamadı!";
            await _logger.LogAsync(new ProductItemActionLog
            {
                Action = "Post",
                Level = "Warn",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                ProductId = dto.ProductId.ToString(),
                Sku = dto.Sku,
                QuantityInStock = dto.QuantityInStock.ToString(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = msg });
        }

        try
        {
            var created = await _service.CreateAsync(dto);
            await _logger.LogAsync(new ProductItemActionLog
            {
                Action = "Post",
                Level = "Info",
                Message = "Ürün stoğu başarıyla eklendi.",
                Timestamp = DateTime.UtcNow,
                ProductId = dto.ProductId.ToString(),
                Sku = dto.Sku,
                QuantityInStock = dto.QuantityInStock.ToString(),
                Description = WrapObject(created)
            });
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Contains("SKU") ? "Aynı SKU ile ürün zaten mevcut." : ex.Message;

            await _logger.LogAsync(new ProductItemActionLog
            {
                Action = "Post",
                Level = "Error",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                ProductId = dto.ProductId.ToString(),
                Sku = dto.Sku,
                QuantityInStock = dto.QuantityInStock.ToString(),
                Description = WrapObject(new { Request = dto, Exception = ex.Message })
            });

            if (ex.Message.Contains("SKU"))
                return Conflict(new { message = msg });

            return BadRequest(new { message = msg });
        }
    }

    /* ---------- PUT /api/productitems/{id} ---------- */
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Ürün stoğunu güncelle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, CreateProductItemDto dto)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        // ProductId kontrolü
        bool productExists = await _productApiClient.ProductExists(dto.ProductId);

        if (!productExists)
        {
            string msg = "Bağlı olduğu ürün bulunamadı!";
            await _logger.LogAsync(new ProductItemActionLog
            {
                Action = "Put",
                Level = "Warn",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                ProductId = dto.ProductId.ToString(),
                Sku = dto.Sku,
                QuantityInStock = dto.QuantityInStock.ToString(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = msg });
        }

        try
        {
            await _service.UpdateAsync(id, dto);

            await _logger.LogAsync(new ProductItemActionLog
            {
                Action = "Put",
                Level = "Info",
                Message = "Ürün stoğu başarıyla güncellendi.",
                Timestamp = DateTime.UtcNow,
                ProductId = dto.ProductId.ToString(),
                Sku = dto.Sku,
                QuantityInStock = dto.QuantityInStock.ToString(),
                Description = WrapObject(dto)
            });

            return NoContent();
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Contains("SKU") ? "Aynı SKU ile ürün zaten mevcut." : ex.Message;

            await _logger.LogAsync(new ProductItemActionLog
            {
                Action = "Put",
                Level = "Error",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                ProductId = dto.ProductId.ToString(),
                Sku = dto.Sku,
                QuantityInStock = dto.QuantityInStock.ToString(),
                Description = WrapObject(new { Request = dto, Exception = ex.Message })
            });

            if (ex.Message.Contains("SKU"))
                return Conflict(new { message = msg });

            return BadRequest(new { message = msg });
        }
    }

    /* ---------- DELETE /api/productitems/{id} ---------- */
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Ürün stoğunu sil")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null)
        {
            await _logger.LogAsync(new ProductItemActionLog
            {
                Action = "Delete",
                Level = "Warn",
                Message = "Silmek istediğin ürün stoğu bulunamadı.",
                Timestamp = DateTime.UtcNow,
                ProductId = null,
                Sku = null,
                QuantityInStock = null,
                Description = WrapObject(new { ProductItemId = id })
            });
            return NotFound();
        }

        await _service.DeleteAsync(id);

        await _logger.LogAsync(new ProductItemActionLog
        {
            Action = "Delete",
            Level = "Info",
            Message = "Ürün stoğu başarıyla silindi.",
            Timestamp = DateTime.UtcNow,
            ProductId = existing.ProductId.ToString(),
            Sku = existing.Sku,
            QuantityInStock = existing.QuantityInStock.ToString(),
            Description = WrapObject(existing)
        });

        return NoContent();
    }
}
