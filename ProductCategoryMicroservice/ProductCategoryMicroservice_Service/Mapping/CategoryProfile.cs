using AutoMapper;
using ProductCategoryMicroservice_Service.DTOs;
using ProductCategoryMicroservice_Data.Entities;

namespace ProductCategoryMicroservice_Service.Mapping;

/// <summary>
/// AutoMapper eşleştirmeleri:
/// • Entity ➜ DTO  (GET/GET ALL)
/// • Create DTO ➜ Entity  (POST)
/// • Update DTO ➜ Entity  (PUT) — null alanlar dokunulmaz
/// </summary>
public sealed class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        // Entity ➜ DTO
        CreateMap<Category, CategoryDto>();

        // Create DTO ➜ Entity
        CreateMap<CreateCategoryDto, Category>();

        // Update DTO ➜ Entity
        CreateMap<UpdateCategoryDto, Category>()
            .ForAllMembers(opts =>
                opts.Condition((src, _, value) => value is not null)); // null gelen alanları atla
    }
}
