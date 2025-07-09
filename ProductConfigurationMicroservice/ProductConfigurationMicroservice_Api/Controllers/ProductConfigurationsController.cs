using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

using ProductConfigurationMicroservice_Service.DTOs;
using ProductConfigurationMicroservice_Service.Interfaces;

namespace ProductConfigurationMicroservice_Api.Controllers;

[ApiController]
[Route("api/productconfigurations")]        // PLURAL → gateway ile aynı
public class ProductConfigurationsController : ControllerBase
{
    private readonly IProductConfigurationService _svc;
    public ProductConfigurationsController(IProductConfigurationService svc) => _svc = svc;

    /*──────────────────────────────────────────────────────
      GET api/productconfigurations?productItemIds=&variationOptionIds=
     ──────────────────────────────────────────────────────*/
    [HttpGet]
    [SwaggerOperation(
        Summary     = "Tüm ürün konfigürasyonlarını getir",
        Description = "ProductItemId ve/veya VariationOptionId’ye göre filtreleme yapılabilir"
    )]
    [SwaggerResponse(200, "Başarılı", typeof(IEnumerable<ProductConfigurationDto>))]
    public async Task<ActionResult<IEnumerable<ProductConfigurationDto>>> GetAll(
        [FromQuery] int[]? productItemIds,
        [FromQuery] int[]? variationOptionIds)
    {
        var list = await _svc.GetAllAsync(productItemIds, variationOptionIds);
        return Ok(list);
    }

    /*──────────────────────────────────────────────────────
      GET api/productconfigurations/{id}
     ──────────────────────────────────────────────────────*/
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "ID’ye göre ürün konfigürasyonu getir")]
    [SwaggerResponse(200, "Bulundu", typeof(ProductConfigurationDto))]
    [SwaggerResponse(404, "Bulunamadı")]
    public async Task<ActionResult<ProductConfigurationDto>> Get(int id)
        => await _svc.GetByIdAsync(id) is { } dto ? Ok(dto) : NotFound();

    /*──────────────────────────────────────────────────────
      POST api/productconfigurations
     ──────────────────────────────────────────────────────*/
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni ürün konfigürasyonu oluştur")]
    [SwaggerResponse(201, "Oluşturuldu", typeof(ProductConfigurationDto))]
    public async Task<ActionResult<ProductConfigurationDto>> Create(
        [FromBody] CreateProductConfigurationDto dto)
    {
        var created = await _svc.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /*──────────────────────────────────────────────────────
      PUT api/productconfigurations/{id}
     ──────────────────────────────────────────────────────*/
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Ürün konfigürasyonunu güncelle")]
    [SwaggerResponse(204, "Güncellendi")]
    public async Task<IActionResult> Update(int id,
        [FromBody] UpdateProductConfigurationDto dto)
    {
        if (id != dto.Id) return BadRequest("Body-ID uyuşmuyor");
        await _svc.UpdateAsync(dto);
        return NoContent();
    }

    /*──────────────────────────────────────────────────────
      DELETE api/productconfigurations/{id}
     ──────────────────────────────────────────────────────*/
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Ürün konfigürasyonu sil")]
    [SwaggerResponse(204, "Silindi")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return NoContent();
    }
}