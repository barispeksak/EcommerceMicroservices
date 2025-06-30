// Mapping/ShippingTypeProfile.cs
using AutoMapper;
using ShippingTypeMicroservice.Entities;
using ShippingTypeMicroservice.Data.Dtos;

namespace ShippingTypeMicroservice.Mapping
{
    public class ShippingTypeProfile : Profile
    {
        public ShippingTypeProfile()
        {
            CreateMap<ShippingType, ShippingTypeDto>().ReverseMap();;
            CreateMap<CreateShippingTypeDto, ShippingType>().ReverseMap();;
            CreateMap<UpdateShippingTypeDto, ShippingType>().ReverseMap();;
        }
    }
}