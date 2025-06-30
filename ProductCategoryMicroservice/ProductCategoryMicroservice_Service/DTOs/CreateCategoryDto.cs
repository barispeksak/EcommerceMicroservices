using Swashbuckle.AspNetCore.Annotations;

namespace ProductCategoryMicroservice_Service.DTOs;

/// <remarks>HTTP POST isteğinde kullanılır.</remarks>
[SwaggerSchema(Description = "Yeni kategori eklemek için gönderilen model.")]
public sealed record CreateCategoryDto
{
    [SwaggerSchema("Kategori adı", Nullable = false)]
    public string CategoryName { get; init; } = default!;

    [SwaggerSchema("Üst kategori ID'si (opsiyonel)")]
    public int? ParentCategoryId { get; init; }
}
