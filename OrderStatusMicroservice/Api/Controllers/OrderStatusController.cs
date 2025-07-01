// Api/Controllers/OrderStatusController.cs
using Microsoft.AspNetCore.Mvc;
using OrderStatusMicroservice.Data.Dtos;
using OrderStatusMicroservice.Services.Interfaces;

namespace OrderStatusMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderStatusController : ControllerBase
{
    private readonly IOrderStatusService _service;

    public OrderStatusController(IOrderStatusService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateOrderStatusDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID uyuşmuyor.");

        var success = await _service.UpdateAsync(dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
