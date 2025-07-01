// ProductConfigurationMicroservice_Service/Mapping/ProductConfigurationProfile.cs
using AutoMapper;
using ProductConfigurationMicroservice_Data.Entities;
using ProductConfigurationMicroservice_Service.DTOs;

namespace ProductConfigurationMicroservice_Service.Mapping;

/// <summary>
/// Entity ↔ DTO eşleştirmeleri.
/// SKU ve VariationOptionName HTTP’den geldiği için AutoMapper bunları IGNORE eder.
/// </summary>
public class ProductConfigurationProfile : Profile
{
    public ProductConfigurationProfile()
    {
        // Entity → Dto
        CreateMap<ProductConfiguration, ProductConfigurationDto>()
            .ForMember(d => d.ProductItemSku,      o => o.Ignore())
            .ForMember(d => d.VariationOptionName, o => o.Ignore());

        // (Opsiyonel) Eğer ileride CreateDto tanımlarsak ters eşleştirme ekleriz.
    }
}
