using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;
using VariationOptionMicroservice.Models;
using VariationOptionMicroservice.Service.DTOs;
using VariationOptionMicroservice.Service.Services;
using VariationOptionMicroservice.Service.Interfaces; // CategoryApiClient burada!

namespace VariationOptionMicroservice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariationOptionController : ControllerBase
    {
        private readonly IVariationOptionService _service;
        private readonly CategoryApiClient _variationApiClient;
        private readonly VariationOptionActionLogger _logger;

        public VariationOptionController(
            IVariationOptionService service,
            CategoryApiClient variationApiClient,
            VariationOptionActionLogger logger)
        {
            _service = service;
            _variationApiClient = variationApiClient;
            _logger = logger;
        }

        private static BsonDocument WrapObject(object? obj) =>
            obj is null
                ? new BsonDocument { { "msg", "null" } }
                : BsonDocument.Parse(JsonSerializer.Serialize(obj));

        private string GetCorrelationId() =>
            HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

        private string GetPerformedByEmail() =>
            HttpContext.Request.Headers["X-User-Email"].FirstOrDefault() ?? "anonymous";

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            var option = await _service.GetAsync(id);

            if (option == null)
            {
                string msg = "Girilen id ile varyasyon seçeneği bulunamadı.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "GetById",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = id.ToString(),
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }

            await _logger.LogAsync(new VariationOptionActionLog
            {
                Action = "GetById",
                Level = "Info",
                Message = "Varyasyon seçeneği getirildi.",
                CorrelationId = cid,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = performedBy,
                VariationId = option.VariationId.ToString(),
                Value = option.Value,
                Description = WrapObject(option)
            });

            return Ok(option);
        }

        [HttpPost]
        public async Task<ActionResult<VariationOptionDto>> Create([FromBody] CreateVariationOptionDto dto)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            // VariationId check - CategoryApiClient üzerinden!
            var variationExists = await _variationApiClient.VariationExists(dto.VariationId);
            if (!variationExists)
            {
                string msg = "Girilen VariationId bulunamadı.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Create",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = dto.VariationId.ToString(),
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            try
            {
                var option = await _service.CreateAsync(dto);

                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Create",
                    Level = "Info",
                    Message = "Varyasyon seçeneği başarıyla oluşturuldu.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = option.VariationId.ToString(),
                    Value = option.Value,
                    Description = WrapObject(option)
                });

                return CreatedAtAction(nameof(GetById), new { id = option.Id }, option);
            }
            catch (Exception ex)
            {
                string msg = "Varyasyon seçeneği oluşturulamadı. Sunucu hatası.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Create",
                    Level = "Error",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = dto.VariationId.ToString(),
                    Description = WrapObject(new { Request = dto, Exception = ex.Message })
                });

                return StatusCode(500, new { message = msg });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVariationOptionDto dto)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            if (id != dto.Id)
            {
                string msg = "Girilen id ile body id uyuşmuyor.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Update",
                    Level = "Error",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = dto.VariationId.ToString(),
                    Description = WrapObject(new { UrlId = id, BodyId = dto.Id })
                });
                return BadRequest(new { message = msg });
            }

            var existing = await _service.GetAsync(id);
            if (existing == null)
            {
                string msg = "Güncellenecek varyasyon seçeneği bulunamadı.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Update",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = dto.VariationId.ToString(),
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }

            // VariationId check - CategoryApiClient üzerinden!
            var variationExists = await _variationApiClient.VariationExists(dto.VariationId);
            if (!variationExists)
            {
                string msg = "Girilen VariationId bulunamadı.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Update",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = dto.VariationId.ToString(),
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            try
            {
                await _service.UpdateAsync(id, dto);

                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Update",
                    Level = "Info",
                    Message = "Varyasyon seçeneği başarıyla güncellendi.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = dto.VariationId.ToString(),
                    Value = dto.Value,
                    Description = WrapObject(dto)
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                string msg = "Varyasyon seçeneği güncellenemedi. Sunucu hatası.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Update",
                    Level = "Error",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = dto.VariationId.ToString(),
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

            var option = await _service.GetAsync(id);
            if (option == null)
            {
                string msg = "Silinecek varyasyon seçeneği bulunamadı.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Delete",
                    Level = "Warn",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = id.ToString(),
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }

            try
            {
                await _service.DeleteAsync(id);

                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Delete",
                    Level = "Info",
                    Message = "Varyasyon seçeneği başarıyla silindi.",
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = option.VariationId.ToString(),
                    Value = option.Value,
                    Description = WrapObject(option)
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                string msg = "Varyasyon seçeneği silinemedi. Sunucu hatası.";
                await _logger.LogAsync(new VariationOptionActionLog
                {
                    Action = "Delete",
                    Level = "Error",
                    Message = msg,
                    CorrelationId = cid,
                    Timestamp = DateTime.UtcNow,
                    PerformedByEmail = performedBy,
                    VariationId = id.ToString(),
                    Description = WrapObject(new { Id = id, Exception = ex.Message })
                });

                return StatusCode(500, new { message = msg });
            }
        }
    }
}
