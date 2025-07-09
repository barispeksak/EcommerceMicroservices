using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OrderStatusMicroservice.Data.Dtos;
using OrderStatusMicroservice.Models;
using OrderStatusMicroservice.Services.Interfaces;
using OrderStatusMicroservice.Services.Logging;

namespace OrderStatusMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderStatusController : ControllerBase
{
    private readonly IOrderStatusService _service;
    private readonly OrderStatusActionLogger _logger;

    public OrderStatusController(IOrderStatusService service, OrderStatusActionLogger logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new OrderStatusActionLog
        {
            CorrelationId = cid,
            Action = "GetAll",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Tüm OrderStatus kayıtları getirildi.",
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
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = cid,
                Action = "GetById",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "OrderStatus bulunamadı.",
                Description = new BsonDocument { { "Id", id } }
            });

            return NotFound();
        }

        await _logger.LogAsync(new OrderStatusActionLog
        {
            CorrelationId = cid,
            Action = "GetById",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "OrderStatus getirildi.",
            Description = new BsonDocument { { "Id", id } }
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreateAsync(dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new OrderStatusActionLog
        {
            CorrelationId = cid,
            Action = "Create",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "OrderStatus oluşturuldu.",
        });

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateOrderStatusDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID uyuşmuyor.");

        var success = await _service.UpdateAsync(dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new OrderStatusActionLog
        {
            CorrelationId = cid,
            Action = "Update",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "OrderStatus güncellendi." : "OrderStatus bulunamadı.",
            Description = new BsonDocument { { "Id", dto.Id } }
        });

        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new OrderStatusActionLog
        {
            CorrelationId = cid,
            Action = "Delete",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "OrderStatus silindi." : "OrderStatus bulunamadı.",
            Description = new BsonDocument { { "Id", id } }
        });

        return success ? NoContent() : NotFound();
    }
}
