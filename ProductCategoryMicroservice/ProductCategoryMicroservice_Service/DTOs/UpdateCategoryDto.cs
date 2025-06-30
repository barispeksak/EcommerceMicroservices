using Swashbuckle.AspNetCore.Annotations;

namespace ProductCategoryMicroservice_Service.DTOs;

/// <remarks>HTTP PUT isteğinde kullanılır.  
/// Null gelen alanlar değiştirilmeyecek (AutoMapper `Condition` kuralıyla).</remarks>
[SwaggerSchema(Description = "Kategoriyi güncellemek için gönderilen model.")]
public sealed record UpdateCategoryDto
{
    [SwaggerSchema("Yeni kategori adı (opsiyonel)")]
    public string? CategoryName { get; init; }

    [SwaggerSchema("Yeni üst kategori ID'si (opsiyonel)")]
    public int? ParentCategoryId { get; init; }
}
