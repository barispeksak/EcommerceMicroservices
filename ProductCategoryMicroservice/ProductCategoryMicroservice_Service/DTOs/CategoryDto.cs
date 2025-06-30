using Swashbuckle.AspNetCore.Annotations;

namespace ProductCategoryMicroservice_Service.DTOs;

/// <remarks>HTTP GET / GET ALL dönüş modelidir.</remarks>
[SwaggerSchema(Description = "Kategori bilgilerini temsil eder (response).")]
public sealed record CategoryDto
{
    [SwaggerSchema("Kategori ID'si", ReadOnly = true)]
    public int Id { get; init; }

    [SwaggerSchema("Kategori adı")]
    public string CategoryName { get; init; } = default!;

    [SwaggerSchema("Üst kategori ID'si (null = kök)")]
    public int? ParentCategoryId { get; init; }
}
