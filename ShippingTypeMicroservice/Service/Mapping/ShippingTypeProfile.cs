// Mapping/ShippingTypeProfile.cs
using AutoMapper;
using ShippingTypeMicroservice.Models;
using ShippingTypeMicroservice.Data.Dtos;

namespace ShippingTypeMicroservice.Mapping
{
    public class ShippingTypeProfile : Profile
    {
        public ShippingTypeProfile()
        {
            CreateMap<ShippingType, ShippingTypeDto>();
            CreateMap<CreateShippingTypeDto, ShippingType>();
            CreateMap<UpdateShippingTypeDto, ShippingType>();
        }
    }
}
