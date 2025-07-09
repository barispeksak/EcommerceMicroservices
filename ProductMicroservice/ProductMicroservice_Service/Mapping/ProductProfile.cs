using AutoMapper;
using ProductMicroservice_Service.DTOs;
using ProductMicroservice_Data.Entities;

namespace ProductMicroservice_Service.Mapping;

/// <summary>
/// Product ↔ DTO dönüşüm kuralları.
/// </summary>
public sealed class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Entity → DTO  (GET / GET ALL)
        CreateMap<Product, ProductDto>();

        // DTO → Entity  (POST)
        CreateMap<CreateProductDto, Product>();

        // DTO (PUT) → Entity – yalnıza NULL olmayan alanları uygula
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, val) => val is not null));
    }
}
