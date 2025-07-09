using Swashbuckle.AspNetCore.Annotations;


namespace ProductMicroservice_Service.DTOs;

/// <remarks>HTTP GET / GET ALL için dönüş modelidir.</remarks>
[SwaggerSchema(Description = "Ürün bilgilerini temsil eder (response).")]
public sealed record ProductDto
{
    [SwaggerSchema("Ürün ID'si", ReadOnly = true)]
    public int Id { get; init; }

    [SwaggerSchema("Ürünün kategori ID'si")]
    public int CategoryId { get; init; }

    [SwaggerSchema("Ürünün adı")]
    public string Name { get; init; } = default!;

    [SwaggerSchema("Ürün açıklaması")]
    public string Description { get; init; } = default!;

    [SwaggerSchema("Ürün görseli URL")]
    public string Image { get; init; } = default!;

    [SwaggerSchema("Marka")]
    public string Brand { get; init; } = default!;
}
