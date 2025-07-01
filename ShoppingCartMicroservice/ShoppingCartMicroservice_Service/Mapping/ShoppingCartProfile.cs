using AutoMapper;
using ShoppingCartMicroservice_Data.Entities;
using ShoppingCartMicroservice_Service.DTOs;

namespace ShoppingCartMicroservice_Service.Mapping
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile()
        {
            CreateMap<CreateShoppingCartDto, ShoppingCart>()
                .ForMember(dest => dest.IsTotalRow, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.LinePrice, opt => opt.Ignore());

            CreateMap<ShoppingCart, ShoppingCartDto>();
        }
    }
}
