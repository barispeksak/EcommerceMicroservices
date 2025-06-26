using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderSummaryController : ControllerBase
{
    private readonly ShopOrderDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderSummaryController(ShopOrderDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    // GET: api/ordersummary
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummary>>> GetAll()
        => await _db.OrderSummaries.ToListAsync();

    // GET: api/ordersummary/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderSummary>> Get(int id)
    {
        var summary = await _db.OrderSummaries.FindAsync(id);
        return summary is null ? NotFound() : summary;
    }

    // POST: api/ordersummary
    [HttpPost]
    public async Task<ActionResult<OrderSummary>> Post(OrderSummary summary)
    {
        // Order kontrolü (veritabanından)
        var orderExists = await _db.ShopOrders.AnyAsync(o => o.Id == summary.OrderId);
        if (!orderExists)
            return BadRequest($"Order ID {summary.OrderId} bulunamadı.");

        // ProductItem kontrolü (başka mikroservisten)
        var client = _httpClientFactory.CreateClient("ProductService");
        var response = await client.GetAsync($"api/productitem/{summary.ProductItemId}");

        if (!response.IsSuccessStatusCode)
            return BadRequest($"ProductItem ID {summary.ProductItemId} başka mikroserviste bulunamadı.");

        _db.OrderSummaries.Add(summary);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = summary.Id }, summary);
    }

    // PUT: api/ordersummary/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, OrderSummary updated)
    {
        if (id != updated.Id) return BadRequest("ID uyuşmuyor.");

        var exists = await _db.OrderSummaries.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        _db.Entry(updated).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/ordersummary/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.OrderSummaries.FindAsync(id);
        if (entity is null) return NotFound();

        _db.OrderSummaries.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
