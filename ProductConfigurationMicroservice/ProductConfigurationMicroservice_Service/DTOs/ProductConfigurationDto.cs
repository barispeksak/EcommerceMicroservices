// ProductConfigurationMicroservice_Service/DTOs/ProductConfigurationDto.cs
namespace ProductConfigurationMicroservice_Service.DTOs;

public record ProductConfigurationDto(
    int Id,
    int ProductItemId,
    int VariationOptionId,
    string ProductItemSku,        // HTTP’den gelir
    string VariationOptionName    // HTTP’den gelir
);
