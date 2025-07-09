using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using VariationMicroservice.Service.DTOs;
using VariationMicroservice.Service.Interfaces;
using VariationMicroservice.Models;
using VariationMicroservice.Service.Services; // Log ve ApiClient için
using System.Text.Json;

namespace VariationMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariationController : ControllerBase
    {
        private readonly IVariationService _variationService;
        private readonly CategoryApiClient _categoryApiClient;
        private readonly VariationActionLogger _logger;

        public VariationController(
            IVariationService variationService,
            CategoryApiClient categoryApiClient,
            VariationActionLogger logger)
        {
            _variationService = variationService;
            _categoryApiClient = categoryApiClient;
            _logger = logger;
        }

        // --- Helpers ---
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

        // --- CRUD ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VariationDto>>> GetAll()
        {
            var variations = await _variationService.GetAllAsync();

            await _logger.LogAsync(new VariationActionLog
            {
                Action = "GetAll",
                Level = "Info",
                Message = "Tüm varyasyonlar listelendi.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                Description = WrapObject(new { Count = variations.Count() }),
                PerformedByEmail = GetPerformedByEmail()
            });

            return Ok(variations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VariationDto>> GetById(int id)
        {
            var variation = await _variationService.GetAsync(id);

            await _logger.LogAsync(new VariationActionLog
            {
                Action = "GetById",
                Level = variation == null ? "Warn" : "Info",
                Message = variation == null ? $"Varyasyon bulunamadı." : "Varyasyon getirildi.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                CategoryId = variation?.CategoryId.ToString(),
                Typename = variation?.VarTypeName,
                Description = variation == null
                    ? WrapObject(new { Error = $"Id: {id} ile varyasyon bulunamadı." })
                    : WrapObject(variation),
                PerformedByEmail = GetPerformedByEmail()
            });

            return variation == null ? NotFound() : Ok(variation);
        }

        [HttpPost]
        public async Task<ActionResult<VariationDto>> Create([FromBody] CreateVariationDto createDto)
        {
            string categoryCheck;
            bool categoryExists;
            try
            {
                try
                {
                    categoryExists = await _categoryApiClient.CategoryExists(createDto.CategoryId);
                    categoryCheck = categoryExists ? "Var" : "Yok";
                }
                catch (Exception ex)
                {
                    categoryCheck = $"Category servisine erişilemedi: {ex.Message}";
                    categoryExists = false;
                }

                if (!categoryExists)
                {
                    await _logger.LogAsync(new VariationActionLog
                    {
                        Action = "Create",
                        Level = "Error",
                        Message = $"Kategori ID {createDto.CategoryId} bulunamadı ya da erişilemedi.",
                        CorrelationId = GetCorrelationId(),
                        Timestamp = DateTime.UtcNow,
                        CategoryId = createDto.CategoryId.ToString(),
                        Typename = createDto.VarTypeName,
                        Description = WrapObject(new
                        {
                            Request = createDto,
                            CategoryCheck = categoryCheck
                        }),
                        PerformedByEmail = GetPerformedByEmail()
                    });
                    return BadRequest($"Kategori ID {createDto.CategoryId} bulunamadı veya erişilemedi.");
                }

                var variation = await _variationService.CreateAsync(createDto);

                await _logger.LogAsync(new VariationActionLog
                {
                    Action = "Create",
                    Level = "Info",
                    Message = $"Varyasyon {variation.Id} başarıyla eklendi.",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    CategoryId = createDto.CategoryId.ToString(),
                    Typename = createDto.VarTypeName,
                    Description = WrapObject(createDto),
                    PerformedByEmail = GetPerformedByEmail()
                });

                return CreatedAtAction(nameof(GetById), new { id = variation.Id }, variation);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(new VariationActionLog
                {
                    Action = "Create",
                    Level = "Error",
                    Message = ex.Message,
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    CategoryId = createDto.CategoryId.ToString(),
                    Typename = createDto.VarTypeName,
                    Description = WrapObject(new { Request = createDto, Exception = ex.Message }),
                    PerformedByEmail = GetPerformedByEmail()
                });
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVariationDto updateDto)
        {
            string categoryCheck;
            bool categoryExists;
            try
            {
                try
                {
                    categoryExists = await _categoryApiClient.CategoryExists(updateDto.CategoryId);
                    categoryCheck = categoryExists ? "Var" : "Yok";
                }
                catch (Exception ex)
                {
                    categoryCheck = $"Category servisine erişilemedi: {ex.Message}";
                    categoryExists = false;
                }

                if (!categoryExists)
                {
                    await _logger.LogAsync(new VariationActionLog
                    {
                        Action = "Update",
                        Level = "Error",
                        Message = $"Kategori ID {updateDto.CategoryId} bulunamadı ya da erişilemedi.",
                        CorrelationId = GetCorrelationId(),
                        Timestamp = DateTime.UtcNow,
                        CategoryId = updateDto.CategoryId.ToString(),
                        Typename = updateDto.VarTypeName,
                        Description = WrapObject(new
                        {
                            Request = updateDto,
                            CategoryCheck = categoryCheck
                        }),
                        PerformedByEmail = GetPerformedByEmail()
                    });
                    return BadRequest($"Kategori ID {updateDto.CategoryId} bulunamadı veya erişilemedi.");
                }

                var result = await _variationService.UpdateAsync(id, updateDto);

                await _logger.LogAsync(new VariationActionLog
                {
                    Action = "Update",
                    Level = result ? "Info" : "Warn",
                    Message = result
                        ? $"Varyasyon {id} başarıyla güncellendi."
                        : $"Varyasyon {id} güncellenemedi.",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    CategoryId = updateDto.CategoryId.ToString(),
                    Typename = updateDto.VarTypeName,
                    Description = WrapObject(updateDto),
                    PerformedByEmail = GetPerformedByEmail()
                });

                return result ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(new VariationActionLog
                {
                    Action = "Update",
                    Level = "Error",
                    Message = ex.Message,
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    CategoryId = updateDto.CategoryId.ToString(),
                    Typename = updateDto.VarTypeName,
                    Description = WrapObject(new { Request = updateDto, Exception = ex.Message }),
                    PerformedByEmail = GetPerformedByEmail()
                });
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var variation = await _variationService.GetAsync(id);

            if (variation == null)
            {
                await _logger.LogAsync(new VariationActionLog
                {
                    Action = "Delete",
                    Level = "Warn",
                    Message = $"Varyasyon ID {id} silinemedi, bulunamadı.",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    CategoryId = null,
                    Typename = null,
                    Description = WrapObject(new { Error = $"Varyasyon {id} bulunamadı." }),
                    PerformedByEmail = GetPerformedByEmail()
                });
                return NotFound();
            }

            await _variationService.DeleteAsync(id);

            await _logger.LogAsync(new VariationActionLog
            {
                Action = "Delete",
                Level = "Info",
                Message = $"Varyasyon {variation.Id} başarıyla silindi.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                CategoryId = variation.CategoryId.ToString(),
                Typename = variation.VarTypeName,
                Description = WrapObject(variation),
                PerformedByEmail = GetPerformedByEmail()
            });

            return NoContent();
        }
    }
}
