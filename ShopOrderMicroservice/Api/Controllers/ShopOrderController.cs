using Microsoft.AspNetCore.Mvc;
using ShopOrderMicroservice.Data.Dtos;
using ShopOrderMicroservice.Services.Interfaces;

namespace ShopOrderMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopOrderController : ControllerBase
{
    private readonly IShopOrderService _service;

    public ShopOrderController(IShopOrderService service)
    {
        _service = service;
    }

    // POST: api/shoporder
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShopOrderDto dto)
    {
        var result = await _service.CreateAsync(dto);
        if (result == null)
            return BadRequest("Sipariş oluşturulamadı. Verilerden biri geçersiz olabilir.");
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // GET: api/shoporder
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    // GET: api/shoporder/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    // PUT: api/shoporder/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShopOrderDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success)
            return BadRequest("Güncelleme başarısız. ID uyuşmazlığı veya bağlı varlıklar geçersiz olabilir.");
        return NoContent();
    }

    // DELETE: api/shoporder/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }

    // GET: api/shoporder/user/3
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var result = await _service.GetByUserIdAsync(userId);
        if (!result.Any())
            return NotFound($"Kullanıcı {userId} için sipariş bulunamadı.");
        return Ok(result);
    }

    // GET: api/shoporder/daterange?start=2024-01-01&end=2024-12-31
    [HttpGet("daterange")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await _service.GetByDateRangeAsync(start, end);
        if (!result.Any())
            return NotFound("Belirtilen tarih aralığında sipariş bulunamadı.");
        return Ok(result);
    }
}
