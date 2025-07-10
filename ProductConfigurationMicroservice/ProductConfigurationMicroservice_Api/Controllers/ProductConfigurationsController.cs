using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ProductConfigurationMicroservice_Service.DTOs;
using ProductConfigurationMicroservice_Service.Interfaces;
using ProductConfigurationMicroservice_Data.Models;
using ProductConfigurationMicroservice_Service.Services; // Logger burada olmalı
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductConfigurationMicroservice_Api.Controllers;

[ApiController]
[Route("api/productconfigurations")]
public class ProductConfigurationsController : ControllerBase
{
    private readonly IProductConfigurationService _svc;
    private readonly ProductConfigurationActionLogger _logger;

    public ProductConfigurationsController(IProductConfigurationService svc, ProductConfigurationActionLogger logger)
    {
        _svc = svc;
        _logger = logger;
    }

    private static BsonDocument WrapObject(object? obj) =>
        obj is null ? new BsonDocument { { "msg", "null" } } : BsonDocument.Parse(JsonSerializer.Serialize(obj));

    private string GetCorrelationId() =>
        HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

    private string GetPerformedByEmail() =>
        HttpContext.Request.Headers["X-User-Email"].FirstOrDefault() ?? "anonymous";

    /*──────────────────────────────────────────────────────
      GET api/productconfigurations?productItemIds=&variationOptionIds=
     ──────────────────────────────────────────────────────*/
    [HttpGet]
    [SwaggerOperation(
        Summary = "Tüm ürün konfigürasyonlarını getir",
        Description = "ProductItemId ve/veya VariationOptionId’ye göre filtreleme yapılabilir"
    )]
    [SwaggerResponse(200, "Başarılı", typeof(IEnumerable<ProductConfigurationDto>))]
    public async Task<ActionResult<IEnumerable<ProductConfigurationDto>>> GetAll(
        [FromQuery] int[]? productItemIds,
        [FromQuery] int[]? variationOptionIds)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();
        var list = await _svc.GetAllAsync(productItemIds, variationOptionIds);

        await _logger.LogAsync(new ProductConfigurationActionLog
        {
            Action = "GetAll",
            Level = "Info",
            Message = "Tüm ürün konfigürasyonları listelendi.",
            CorrelationId = cid,
            Timestamp = DateTime.UtcNow,
            PerformedByEmail = performedBy,
            Description = WrapObject(list)
        });

        return Ok(list);
    }

    /*──────────────────────────────────────────────────────
      GET api/productconfigurations/{id}
     ──────────────────────────────────────────────────────*/
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "ID’ye göre ürün konfigürasyonu getir")]
    [SwaggerResponse(200, "Bulundu", typeof(ProductConfigurationDto))]
    [SwaggerResponse(404, "Bulunamadı")]
    public async Task<ActionResult<ProductConfigurationDto>> Get(int id)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();
        var config = await _svc.GetByIdAsync(id);

        if (config == null)
        {
            string msg = "Girilen ID ile ürün konfigürasyonu bulunamadı.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "GetById",
                Level = "Warn",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = null,
                VariationOptionId = null,
                Description = WrapObject(new { Id = id })
            });
            return NotFound(new { message = msg });
        }

        await _logger.LogAsync(new ProductConfigurationActionLog
        {
            Action = "GetById",
            Level = "Info",
            Message = "Ürün konfigürasyonu getirildi.",
            CorrelationId = cid,
            Timestamp = DateTime.UtcNow,
            PerformedByEmail = performedBy,
            ProductItemId = config.ProductItemId.ToString(),
            VariationOptionId = config.VariationOptionId.ToString(),
            Description = WrapObject(config)
        });

        return Ok(config);
    }

    /*──────────────────────────────────────────────────────
      POST api/productconfigurations
     ──────────────────────────────────────────────────────*/
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni ürün konfigürasyonu oluştur")]
    [SwaggerResponse(201, "Oluşturuldu", typeof(ProductConfigurationDto))]
    public async Task<ActionResult<ProductConfigurationDto>> Create([FromBody] CreateProductConfigurationDto dto)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();

        var (itemExists, sku) = await _svc.ProductItemExistsAsync(dto.ProductItemId);
        if (!itemExists)
        {
            string msg = $"ProductItem ID {dto.ProductItemId} bulunamadı.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Create",
                Level = "Error",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = msg });
        }

        var (optionExists, value) = await _svc.VariationOptionExistsAsync(dto.VariationOptionId);
        if (!optionExists)
        {
            string msg = $"VariationOption ID {dto.VariationOptionId} bulunamadı.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Create",
                Level = "Error",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = msg });
        }

        try
        {
            var created = await _svc.AddAsync(dto);
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Create",
                Level = "Info",
                Message = "Ürün konfigürasyonu başarıyla oluşturuldu.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = created.ProductItemId.ToString(),
                VariationOptionId = created.VariationOptionId.ToString(),
                Description = WrapObject(created)
            });
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            string msg = "Ürün konfigürasyonu oluşturulamadı. Sunucu hatası.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Create",
                Level = "Error",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(new { Request = dto, Exception = ex.Message })
            });
            return StatusCode(500, new { message = msg });
        }
    }

    /*──────────────────────────────────────────────────────
      PUT api/productconfigurations/{id}
     ──────────────────────────────────────────────────────*/
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Ürün konfigürasyonunu güncelle")]
    [SwaggerResponse(200, "Güncellendi", typeof(ProductConfigurationDto))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductConfigurationDto dto)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();

        if (id != dto.Id)
            return BadRequest("Body-ID uyuşmuyor");

        var (itemExists, sku) = await _svc.ProductItemExistsAsync(dto.ProductItemId);
        if (!itemExists)
        {
            string msg = $"ProductItem ID {dto.ProductItemId} bulunamadı.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Update",
                Level = "Fail",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = msg });
        }

        var (optionExists, value) = await _svc.VariationOptionExistsAsync(dto.VariationOptionId);
        if (!optionExists)
        {
            string msg = $"VariationOption ID {dto.VariationOptionId} bulunamadı.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Update",
                Level = "Fail",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = msg });
        }

        try
        {
            await _svc.UpdateAsync(dto);

            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Update",
                Level = "Info",
                Message = "Ürün konfigürasyonu başarıyla güncellendi.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(dto)
            });

            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            string msg = "Güncellenecek ürün konfigürasyonu bulunamadı.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Update",
                Level = "Fail",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(dto)
            });
            return NotFound(new { message = msg });
        }
        catch (Exception ex)
        {
            string msg = "Ürün konfigürasyonu güncellenemedi. Sunucu hatası.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Update",
                Level = "Fail",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = dto.ProductItemId.ToString(),
                VariationOptionId = dto.VariationOptionId.ToString(),
                Description = WrapObject(new { Request = dto, Exception = ex.Message })
            });
            return StatusCode(500, new { message = msg });
        }
    }

    /*──────────────────────────────────────────────────────
      DELETE api/productconfigurations/{id}
     ──────────────────────────────────────────────────────*/
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Ürün konfigürasyonu sil")]
    [SwaggerResponse(204, "Silindi")]
    public async Task<IActionResult> Delete(int id)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();

        var config = await _svc.GetByIdAsync(id);
        if (config == null)
        {
            string msg = "Silinecek ürün konfigürasyonu bulunamadı.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Delete",
                Level = "Fail",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = null,
                VariationOptionId = null,
                Description = WrapObject(new { Id = id })
            });
            return NotFound(new { message = msg });
        }

        try
        {
            await _svc.DeleteAsync(id);

            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Delete",
                Level = "Info",
                Message = "Ürün konfigürasyonu başarıyla silindi.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = config.ProductItemId.ToString(),
                VariationOptionId = config.VariationOptionId.ToString(),
                Description = WrapObject(config)
            });

            return NoContent();
        }
        catch (Exception ex)
        {
            string msg = "Ürün konfigürasyonu silinemedi. Sunucu hatası.";
            await _logger.LogAsync(new ProductConfigurationActionLog
            {
                Action = "Delete",
                Level = "Fail",
                Message = msg,
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                ProductItemId = config.ProductItemId.ToString(),
                VariationOptionId = config.VariationOptionId.ToString(),
                Description = WrapObject(new { Id = id, Exception = ex.Message })
            });
            return StatusCode(500, new { message = msg });
        }
    }
}
