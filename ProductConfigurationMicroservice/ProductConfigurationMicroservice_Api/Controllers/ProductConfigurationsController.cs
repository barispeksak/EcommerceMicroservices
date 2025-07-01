using Microsoft.AspNetCore.Mvc;
using ProductConfigurationMicroservice_Service.DTOs;
using ProductConfigurationMicroservice_Service.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductConfigurationMicroservice_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductConfigurationsController : ControllerBase
{
    private readonly IProductConfigurationService _svc;
    public ProductConfigurationsController(IProductConfigurationService svc) => _svc = svc;

    /// <summary>
    /// Belirtilen ProductItem ve VariationOption ID'lerine göre ürün konfigürasyonlarını listeler.
    /// </summary>
    /// <param name="productItemIds">Filtrelemek için ürün öğesi ID’leri (opsiyonel)</param>
    /// <param name="variationOptionIds">Filtrelemek için varyasyon seçeneği ID’leri (opsiyonel)</param>
    /// <returns>Filtreye uyan tüm ürün konfigürasyonları</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Tüm ürün konfigürasyonlarını getir",
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

    /// <summary>
    /// Belirli bir ürün konfigürasyonunu ID’ye göre getirir.
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "ID’ye göre ürün konfigürasyonu getir",
        Description = "Verilen ID’ye karşılık gelen tek bir konfigürasyon getirir"
    )]
    [SwaggerResponse(200, "Bulundu", typeof(ProductConfigurationDto))]
    [SwaggerResponse(404, "Bulunamadı")]
    public async Task<ActionResult<ProductConfigurationDto>> Get(int id)
        => await _svc.GetByIdAsync(id) is { } dto ? Ok(dto) : NotFound();
        


    /// <summary>Yeni ürün konfigürasyonu ekler.</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni ürün konfigürasyonu oluştur")]
    [SwaggerResponse(201, "Oluşturuldu", typeof(ProductConfigurationDto))]
    public async Task<ActionResult<ProductConfigurationDto>> Create(
        [FromBody] CreateProductConfigurationDto dto)
    {
        var created = await _svc.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Var olan konfigürasyonu günceller.</summary>
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

    /// <summary>
    /// Belirli bir ürün konfigürasyonunu siler.
    /// </summary>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Ürün konfigürasyonu sil",
        Description = "Verilen ID’ye karşılık gelen ürün konfigürasyonunu kalıcı olarak siler"
    )]
    [SwaggerResponse(204, "Silindi")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return NoContent();
    }
}
