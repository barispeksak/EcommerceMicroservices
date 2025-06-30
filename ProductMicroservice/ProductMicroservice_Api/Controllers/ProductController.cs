using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ProductMicroservice.Service.DTOs;
using ProductMicroservice.Service.Interfaces;

namespace ProductMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    /* ---------- GET /api/products/{id} ---------- */
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Tek ürünü getir")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> Get(int id)
        => Ok(await _service.GetAsync(id));

    /* ---------- GET /api/products ---------- */
    [HttpGet]
    [SwaggerOperation(Summary = "Tüm ürünleri getir")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<ProductDto>> GetAll()
        => await _service.GetAllAsync();

    /* ---------- POST /api/products ---------- */
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni ürün ekle")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> Post(CreateProductDto dto)
    {
       try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /* ---------- PUT /api/products/{id} ---------- */
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Ürünü güncelle")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> Put(int id, UpdateProductDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /* ---------- DELETE /api/products/{id} ---------- */
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Ürünü sil")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
