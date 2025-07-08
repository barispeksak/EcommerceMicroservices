using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ProductCategoryMicroservice_Service.DTOs;
using ProductCategoryMicroservice_Service.Interfaces;
using ProductCategoryMicroservice_Data.Models;
using ProductCategoryMicroservice_Service.Services;
using System.Text.Json;

namespace ProductCategoryMicroservice_Api.Controllers;

[ApiController]
[Route("api/productcategory")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly ProductCategoryActionLogger _logger;

    public CategoriesController(ICategoryService service, ProductCategoryActionLogger logger)
    {
        _service = service;
        _logger = logger;
    }

    // -------- Yardımcılar --------

    private static BsonDocument WrapString(string msg) =>
        new() { { "msg", msg } };

    private static BsonDocument WrapObject(object? obj) =>
        obj is null
            ? new BsonDocument { { "msg", "null" } }
            : BsonDocument.Parse(JsonSerializer.Serialize(obj));

    private string GetCorrelationId() =>
        HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

    private string GetPerformedByEmail()
    {
        var email = HttpContext.Request.Headers["X-User-Email"].FirstOrDefault();
        return email ?? "anonymous";
    }

    // -------- CRUD --------

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var dto = await _service.GetAsync(id);

        await _logger.LogAsync(new ProductCategoryActionLog
        {
            Action = "Get",
            Level = dto == null ? "Warn" : "Info",
            Message = dto == null ? "Kategori bulunamadı." : "Kategori getirildi.",
            CorrelationId = GetCorrelationId(),
            Timestamp = DateTime.UtcNow,
            ProductCategoryId = (dto?.Id ?? id).ToString(),
            Name = dto?.CategoryName,
            ParentCategoryId = dto?.ParentCategoryId?.ToString(),
            Description = dto == null
                ? WrapString($"Id: {id} ile kategori bulunamadı.")
                : WrapObject(dto),
            PerformedByEmail = GetPerformedByEmail()
        });

        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    public async Task<IEnumerable<CategoryDto>> GetAll()
    {
        var all = await _service.GetAllAsync();

        await _logger.LogAsync(new ProductCategoryActionLog
        {
            Action = "GetAll",
            Level = "Info",
            Message = "Tüm kategoriler listelendi.",
            CorrelationId = GetCorrelationId(),
            Timestamp = DateTime.UtcNow,
            Description = WrapString($"Toplam kategori sayısı: {all.Count()}"),
            PerformedByEmail = GetPerformedByEmail()
        });

        return all;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Post(CreateCategoryDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            string msg = dto.ParentCategoryId == null
                ? $"Parent category, isim: {created.CategoryName} ve id: {created.Id} ile başarıyla eklendi."
                : $"Parent category id'si {dto.ParentCategoryId} olan kategori, isim:{created.CategoryName} ve id:{created.Id} ile başarıyla eklendi.";

            await _logger.LogAsync(new ProductCategoryActionLog
            {
                Action = "Create",
                Level = "Info",
                Message = msg,
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                ProductCategoryId = created.Id.ToString(),
                Name = created.CategoryName,
                ParentCategoryId = dto.ParentCategoryId?.ToString(),
                Description = WrapObject(dto),
                PerformedByEmail = GetPerformedByEmail()
            });

            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new ProductCategoryActionLog
            {
                Action = "Create",
                Level = "Error",
                Message = ex.Message,
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                Name = dto.CategoryName,
                ParentCategoryId = dto.ParentCategoryId?.ToString(),
                Description = new BsonDocument
                {
                    { "dto", JsonSerializer.Serialize(dto) },
                    { "exception", ex.Message }
                },
                PerformedByEmail = GetPerformedByEmail()
            });

            if (ex.Message.Contains("Kategori adı zaten mevcut."))
                return Conflict(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UpdateCategoryDto dto)
    {
        var existing = await _service.GetAsync(id);
        if (existing is null)
        {
            await _logger.LogAsync(new ProductCategoryActionLog
            {
                Action = "Update",
                Level = "Warn",
                Message = $"Kategori ID {id} güncellenemedi, bulunamadı.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                ProductCategoryId = id.ToString(),
                Description = WrapObject(dto),
                PerformedByEmail = GetPerformedByEmail()
            });
            return NotFound();
        }

        await _service.UpdateAsync(id, dto);

        await _logger.LogAsync(new ProductCategoryActionLog
        {
            Action = "Update",
            Level = "Info",
            Message = $"Kategori {existing.CategoryName}, {existing.Id} başarıyla güncellendi.",
            CorrelationId = GetCorrelationId(),
            Timestamp = DateTime.UtcNow,
            ProductCategoryId = existing.Id.ToString(),
            Name = existing.CategoryName,
            ParentCategoryId = existing.ParentCategoryId?.ToString(),
            Description = WrapObject(dto),
            PerformedByEmail = GetPerformedByEmail()
        });

        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetAsync(id);
        if (existing is null)
        {
            await _logger.LogAsync(new ProductCategoryActionLog
            {
                Action = "Delete",
                Level = "Warn",
                Message = $"Kategori ID {id} silinemedi, bulunamadı.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                ProductCategoryId = id.ToString(),
                Description = WrapString($"Kategori {id} bulunamadı."),
                PerformedByEmail = GetPerformedByEmail()
            });
            return NotFound();
        }

        await _service.DeleteAsync(id);

        await _logger.LogAsync(new ProductCategoryActionLog
        {
            Action = "Delete",
            Level = "Info",
            Message = $"Kategori {existing.CategoryName}, {existing.Id} başarıyla silindi.",
            CorrelationId = GetCorrelationId(),
            Timestamp = DateTime.UtcNow,
            ProductCategoryId = existing.Id.ToString(),
            Name = existing.CategoryName,
            ParentCategoryId = existing.ParentCategoryId?.ToString(),
            Description = WrapObject(existing),
            PerformedByEmail = GetPerformedByEmail()
        });

        return Ok(existing);
    }
}
