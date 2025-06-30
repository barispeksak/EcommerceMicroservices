using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ProductItemMicroservice_Service.DTOs;
using ProductItemMicroservice_Service.Interfaces;

namespace ProductItemMicroservice_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductItemsController : ControllerBase
{
    private readonly IProductItemService _service;
    public ProductItemsController(IProductItemService service) => _service = service;

    /* ---------- GET /api/productitems/{id} ---------- */
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Tek ürün stoğunu getir")]
    [ProducesResponseType(typeof(ProductItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductItemDto>> Get(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /* ---------- GET /api/productitems ---------- */
    [HttpGet]
    [SwaggerOperation(Summary = "Tüm ürün stoklarını getir")]
    [ProducesResponseType(typeof(IEnumerable<ProductItemDto>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<ProductItemDto>> GetAll()
        => await _service.GetAllAsync();

    /* ---------- POST /api/productitems ---------- */
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni ürün stoğu ekle")]
    [ProducesResponseType(typeof(ProductItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductItemDto>> Post(CreateProductItemDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            // Duplicate SKU → 409, diğer iş kuralı hataları → 400
            if (ex.Message.Contains("SKU"))
                return Conflict(new { message = ex.Message });
            
            if (ex.Message.Contains("ürün", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("product", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = ex.Message }); 

            return BadRequest(new { message = ex.Message });
        }
    }

    /* ---------- PUT /api/productitems/{id} ---------- */
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Ürün stoğunu güncelle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, CreateProductItemDto dto)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        try
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("SKU"))
                return Conflict(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
    }

    /* ---------- DELETE /api/productitems/{id} ---------- */
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Ürün stoğunu sil")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
