using AutoMapper;
using ProductItemMicroservice_Data.Entities;
using ProductItemMicroservice_Service.DTOs;

namespace ProductItemMicroservice_Service.Mapping
{
    public class ProductItemProfile : Profile
    {
        public ProductItemProfile()
        {
            // Entity -> DTO
            CreateMap<ProductItem, ProductItemDto>();

            // DTO -> Entity (Create ve Update için)
            CreateMap<CreateProductItemDto, ProductItem>();
        }
    }
}
