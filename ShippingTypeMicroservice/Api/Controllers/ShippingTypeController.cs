using Microsoft.AspNetCore.Mvc;
using ShippingTypeMicroservice.Data.Dtos;
using ShippingTypeMicroservice.Services.Interfaces;
using ShippingTypeMicroservice.Service.Logging;
using ShippingTypeMicroservice.Models;
using MongoDB.Bson;

namespace ShippingTypeMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingTypeController : ControllerBase
{
    private readonly IShippingTypeService _service;
    private readonly ShippingActionLogger _logger;

    public ShippingTypeController(IShippingTypeService service, ShippingActionLogger logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new ShippingActionLog
        {
            CorrelationId = cid,
            Action = "GetAll",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Tüm shipping type'lar getirildi.",
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
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                Action = "GetById",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "ShippingType bulunamadı.",
                Description = new BsonDocument { { "Id", id } }
            });

            return NotFound();
        }

        await _logger.LogAsync(new ShippingActionLog
        {
            CorrelationId = cid,
            Action = "GetById",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "ShippingType getirildi.",
            Description = new BsonDocument { { "Id", id } }
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateShippingTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreateAsync(dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new ShippingActionLog
        {
            CorrelationId = cid,
            Action = "Create",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "ShippingType oluşturuldu.",
            Description = new BsonDocument
            {
                { "Type", dto.Type },
                { "Price", dto.Price }
            }
        });

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateShippingTypeDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID uyuşmuyor.");

        var success = await _service.UpdateAsync(dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new ShippingActionLog
        {
            CorrelationId = cid,
            Action = "Update",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "ShippingType güncellendi." : "ShippingType bulunamadı.",
            Description = new BsonDocument
            {
                { "Id", dto.Id },
                { "Type", dto.Type },
                { "Price", dto.Price }
            }
        });

        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new ShippingActionLog
        {
            CorrelationId = cid,
            Action = "Delete",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "ShippingType silindi." : "ShippingType bulunamadı.",
            Description = new BsonDocument { { "Id", id } }
        });

        return success ? NoContent() : NotFound();
    }
}
