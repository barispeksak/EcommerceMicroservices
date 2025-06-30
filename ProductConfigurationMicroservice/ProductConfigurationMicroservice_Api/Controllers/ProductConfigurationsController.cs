using Microsoft.AspNetCore.Mvc;
using ProductConfigurationMicroservice_Service.DTOs;
using ProductConfigurationMicroservice_Service.Interfaces;

namespace ProductConfigurationMicroservice_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductConfigurationsController : ControllerBase
{
    private readonly IProductConfigurationService _svc;
    public ProductConfigurationsController(IProductConfigurationService svc) => _svc = svc;

    // GET /api/product-configurations?productItemId=1011
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductConfigurationDto>>> GetAll([FromQuery] int? productItemId)
        => Ok(await _svc.GetAllAsync(productItemId));

    // GET /api/product-configurations/42
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductConfigurationDto>> Get(int id)
        => await _svc.GetByIdAsync(id) is { } dto ? Ok(dto) : NotFound();

    // DELETE /api/product-configurations/42
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return NoContent();
    }
}
