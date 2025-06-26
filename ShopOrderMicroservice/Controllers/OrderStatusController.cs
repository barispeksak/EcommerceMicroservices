using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderStatusController : ControllerBase
{
    private readonly ShopOrderDbContext _db;
    public OrderStatusController(ShopOrderDbContext db) => _db = db;

    // GET: api/orderstatus
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderStatus>>> GetAll()
        => await _db.OrderStatuses.ToListAsync();

    // GET: api/orderstatus/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderStatus>> Get(int id)
    {
        var entity = await _db.OrderStatuses.FindAsync(id);
        return entity is null ? NotFound() : entity;
    }

    // POST: api/orderstatus
    [HttpPost]
    public async Task<ActionResult<OrderStatus>> Post(OrderStatus orderStatus)
    {
        _db.OrderStatuses.Add(orderStatus);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = orderStatus.Id }, orderStatus);
    }

    // PUT: api/orderstatus/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, OrderStatus updated)
    {
        if (id != updated.Id) return BadRequest();

        var exists = await _db.OrderStatuses.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        _db.Entry(updated).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/orderstatus/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.OrderStatuses.FindAsync(id);
        if (entity is null) return NotFound();

        _db.OrderStatuses.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
