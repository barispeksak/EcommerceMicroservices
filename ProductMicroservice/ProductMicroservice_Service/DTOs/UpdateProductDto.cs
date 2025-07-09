using Swashbuckle.AspNetCore.Annotations;

namespace ProductMicroservice_Service.DTOs;

/// <remarks>HTTP PUT isteğinde kullanılır.  
/// Null gelen alanlar değiştirilmeyecek (AutoMapper `Condition` kuralı ile).</remarks>
[SwaggerSchema(Description = "Ürünü güncellemek için gönderilen model.")]
public sealed record UpdateProductDto
{
    [SwaggerSchema("Yeni kategori kimliği (opsiyonel)")]
    public int? CategoryId { get; init; }

    [SwaggerSchema("Yeni ürün adı (opsiyonel)")]
    public string? Name { get; init; }

    [SwaggerSchema("Yeni açıklama (opsiyonel)")]
    public string? Description { get; init; }

    [SwaggerSchema("Yeni görsel URL (opsiyonel)")]
    public string? Image { get; init; }

    [SwaggerSchema("Yeni marka (opsiyonel)")]
    public string? Brand { get; init; }
}
