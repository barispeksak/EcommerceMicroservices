using AutoMapper;
using ShoppingCartMicroservice_Service.DTOs;

namespace ShoppingCartMicroservice_Service.Mapping
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile()
        {
            // Örneğin ileride gerekirse
            CreateMap<CreateShoppingCartDto, ShoppingCartItemDto>();
            CreateMap<ShoppingCartItemDto, CartItemDetailsDto>();
        }
    }
}
