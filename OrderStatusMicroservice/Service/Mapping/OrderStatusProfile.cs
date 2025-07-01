// Mapping/OrderStatusProfile.cs
using AutoMapper;
using OrderStatusMicroservice.Entities;
using OrderStatusMicroservice.Data.Dtos;

namespace OrderStatusMicroservice.Mapping
{
    public class OrderStatusProfile : Profile
    {
        public OrderStatusProfile()
        {
            CreateMap<OrderStatus, OrderStatusDto>().ReverseMap();;
            CreateMap<CreateOrderStatusDto, OrderStatus>().ReverseMap();;
            CreateMap<UpdateOrderStatusDto, OrderStatus>().ReverseMap();;
        }
    }
}