// Controllers/ShippingTypeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShippingTypeController : ControllerBase
{
    private readonly ShopOrderDbContext _db;

    public ShippingTypeController(ShopOrderDbContext db) => _db = db;

    // GET: api/shippingtype
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShippingType>>> GetAll()
        => await _db.ShippingTypes.ToListAsync();

    // GET: api/shippingtype/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShippingType>> Get(int id)
    {
        var entity = await _db.ShippingTypes.FindAsync(id);
        return entity is null ? NotFound() : entity;
    }

    // POST: api/shippingtype
    [HttpPost]
    public async Task<ActionResult<ShippingType>> Post(ShippingType shippingType)
    {
        _db.ShippingTypes.Add(shippingType);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = shippingType.Id }, shippingType);
    }

    // PUT: api/shippingtype/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, ShippingType updated)
    {
        if (id != updated.Id) return BadRequest("ID uyuşmuyor.");

        var exists = await _db.ShippingTypes.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        _db.Entry(updated).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/shippingtype/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.ShippingTypes.FindAsync(id);
        if (entity is null) return NotFound();

        _db.ShippingTypes.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
