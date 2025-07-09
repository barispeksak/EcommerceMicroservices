using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PaymentTypeMicroservice.Data.Dtos;
using PaymentTypeMicroservice.Models;
using PaymentTypeMicroservice.Services.Logging;
using PaymentTypeMicroservice.Services.Interfaces;
using System.Text.Json;

namespace PaymentTypeMicroservice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IPaymentTypeService _service;
        private readonly PaymentTypeActionLogger _logger;

        public PaymentTypeController(IPaymentTypeService service, PaymentTypeActionLogger logger)
        {
            _service = service;
            _logger = logger;
        }

        // -- Helpers --
        private static BsonDocument WrapObject(object? obj) =>
            obj is null
                ? new BsonDocument { { "msg", "null" } }
                : BsonDocument.Parse(JsonSerializer.Serialize(obj));

        private string GetCorrelationId() =>
            HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

        private string GetPerformedByEmail() =>
            HttpContext.Request.Headers["X-User-Email"].FirstOrDefault() ?? "anonymous";

        // -- CRUD Endpoints --

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            await _logger.LogAsync(new PaymentTypeActionLog
            {
                CorrelationId = cid,
                Action = "GetAll",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Tüm ödeme tipleri listelendi.",
                PerformedByEmail = performedBy,
                Description = WrapObject(new {
                    Count = result.Count(),
                    PaymentTypes = result
                })
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            if (result == null)
            {
                string msg = "Girilen id ile ödeme tipi bulunamadı.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "GetById",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentTypeId = id,
                    Description = WrapObject(new { Id = id })
                });

                return NotFound(new { message = msg });
            }

            await _logger.LogAsync(new PaymentTypeActionLog
            {
                CorrelationId = cid,
                Action = "GetById",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Ödeme tipi getirildi.",
                PerformedByEmail = performedBy,
                PaymentTypeId = result.Id,
                PaymentType = result.Type,
                Description = WrapObject(result)
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentTypeDto dto)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            // Aynı isimde payment type var mı?
            var exists = await _service.ExistsByNameAsync(dto.Type);
            if (exists)
            {
                string msg = "Aynı isimde bir ödeme tipi zaten mevcut.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Create",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentType = dto.Type,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            try
            {
                var created = await _service.CreateAsync(dto);

                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Create",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Message = "Ödeme tipi başarıyla oluşturuldu.",
                    PerformedByEmail = performedBy,
                    PaymentTypeId = created.Id,
                    PaymentType = created.Type,
                    Description = WrapObject(created)
                });

                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                string msg = "Ödeme tipi oluşturulamadı. Sunucu hatası.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Create",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentType = dto.Type,
                    Description = WrapObject(new { Request = dto, Exception = ex.Message })
                });
                return StatusCode(500, new { message = msg });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentTypeDto dto)
        {
            var cid = GetCorrelationId();
            var performedBy = GetPerformedByEmail();

            if (id != dto.Id)
            {
                string msg = "Girilen id ile body id uyuşmuyor.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Update",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentTypeId = id,
                    Description = WrapObject(new { UrlId = id, BodyId = dto.Id })
                });
                return BadRequest(new { message = msg });
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                string msg = "Güncellenecek ödeme tipi bulunamadı.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Update",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentTypeId = id,
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }

            try
            {
                await _service.UpdateAsync(dto);

                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Update",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Message = "Ödeme tipi başarıyla güncellendi.",
                    PerformedByEmail = performedBy,
                    PaymentTypeId = dto.Id,
                    PaymentType = dto.Type,
                    Description = WrapObject(dto)
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                string msg = "Ödeme tipi güncellenemedi. Sunucu hatası.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Update",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentTypeId = dto.Id,
                    PaymentType = dto.Type,
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

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                string msg = "Silinecek ödeme tipi bulunamadı.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Delete",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentTypeId = id,
                    Description = WrapObject(new { Id = id })
                });
                return NotFound(new { message = msg });
            }

            try
            {
                await _service.DeleteAsync(id);

                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Delete",
                    Timestamp = DateTime.UtcNow,
                    Status = "Success",
                    Message = "Ödeme tipi başarıyla silindi.",
                    PerformedByEmail = performedBy,
                    PaymentTypeId = existing.Id,
                    PaymentType = existing.Type,
                    Description = WrapObject(existing)
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                string msg = "Ödeme tipi silinemedi. Sunucu hatası.";
                await _logger.LogAsync(new PaymentTypeActionLog
                {
                    CorrelationId = cid,
                    Action = "Delete",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = msg,
                    PerformedByEmail = performedBy,
                    PaymentTypeId = id,
                    Description = WrapObject(new { Id = id, Exception = ex.Message })
                });
                return StatusCode(500, new { message = msg });
            }
        }
    }
}
