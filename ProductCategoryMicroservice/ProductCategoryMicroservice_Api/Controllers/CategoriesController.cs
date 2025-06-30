using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ProductCategoryMicroservice_Service.DTOs;
using ProductCategoryMicroservice_Service.Interfaces;

namespace ProductCategoryMicroservice_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) => _service = service;

    /* ---------- GET /api/categories/{id} ---------- */
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Tek kategoriyi getir")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var dto = await _service.GetAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /* ---------- GET /api/categories ---------- */
    [HttpGet]
    [SwaggerOperation(Summary = "Tüm kategorileri getir")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<CategoryDto>> GetAll()
        => await _service.GetAllAsync();

    /* ---------- POST /api/categories ---------- */
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni kategori ekle")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> Post(CreateCategoryDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            // Exception message duplicate ise 409, diğerleri için 400
            if (ex.Message.Contains("Kategori adı zaten mevcut. Başka bir isim deneyin."))
                return Conflict(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
    }

    /* ---------- PUT /api/categories/{id} ---------- */
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Kategoriyi güncelle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, UpdateCategoryDto dto)
    {
        var existing = await _service.GetAsync(id);
        if (existing is null) return NotFound();

        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    /* ---------- DELETE /api/categories/{id} ---------- */
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Kategoriyi sil")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetAsync(id);
        if (existing is null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
