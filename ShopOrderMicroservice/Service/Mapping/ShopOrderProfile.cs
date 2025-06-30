using AutoMapper;
using ShopOrderMicroservice.Data.Dtos;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Mapping
{
    public class ShopOrderProfile : Profile
    {
        public ShopOrderProfile()
        {
            CreateMap<CreateShopOrderDto, ShopOrder>();
            CreateMap<UpdateShopOrderDto, ShopOrder>();
            CreateMap<ShopOrder, ShopOrderDto>();
        }
    }
}
