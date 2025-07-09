using Swashbuckle.AspNetCore.Annotations;

namespace ProductMicroservice_Service.DTOs;

/// <remarks>HTTP POST isteğinde kullanılır.</remarks>
[SwaggerSchema(Description = "Yeni ürün eklemek için gönderilen model.")]
public sealed record CreateProductDto
{
    [SwaggerSchema("Kategori ID'si ")]
    public int CategoryId { get; init; }

    [SwaggerSchema("Ürün adı", Nullable = false)]
    public string Name { get; init; } = default!;

    [SwaggerSchema("Ürün açıklaması", Nullable = false)]
    public string Description { get; init; } = default!;

    [SwaggerSchema("Görsel URL", Nullable = false)]
    public string Image { get; init; } = default!;

    [SwaggerSchema("Marka", Nullable = false)]
    public string Brand { get; init; } = default!;
}
