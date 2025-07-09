using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PaymentTypeMicroservice.Data.Dtos;
using PaymentTypeMicroservice.Models;
using PaymentTypeMicroservice.Services.Logging;
using PaymentTypeMicroservice.Services.Interfaces;

namespace PaymentTypeMicroservice.Api.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new PaymentTypeActionLog
        {
            CorrelationId = cid,
            Action = "GetAll",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Tüm payment type'lar getirildi.",
            Description = new BsonDocument { { "Count", result.Count() } }
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetByIdAsync(id);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        if (result == null)
        {
            await _logger.LogAsync(new PaymentTypeActionLog
            {
                CorrelationId = cid,
                Action = "GetById",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "PaymentType bulunamadı.",
                Description = new BsonDocument { { "Id", id } }
            });

            return NotFound();
        }

        await _logger.LogAsync(new PaymentTypeActionLog
        {
            CorrelationId = cid,
            Action = "GetById",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "PaymentType getirildi.",
            Description = new BsonDocument { { "Id", id } }
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreateAsync(dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new PaymentTypeActionLog
        {
            CorrelationId = cid,
            Action = "Create",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "PaymentType oluşturuldu.",
            Description = new BsonDocument { { "Type", dto.Type } }
        });

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePaymentTypeDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID uyuşmuyor.");

        var success = await _service.UpdateAsync(dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new PaymentTypeActionLog
        {
            CorrelationId = cid,
            Action = "Update",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "PaymentType güncellendi." : "PaymentType bulunamadı.",
            Description = new BsonDocument { { "Id", dto.Id }, { "Type", dto.Type } }
        });

        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new PaymentTypeActionLog
        {
            CorrelationId = cid,
            Action = "Delete",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "PaymentType silindi." : "PaymentType bulunamadı.",
            Description = new BsonDocument { { "Id", id } }
        });

        return success ? NoContent() : NotFound();
    }
}
