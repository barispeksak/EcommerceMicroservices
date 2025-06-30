using AutoMapper;
using VariationMicroservice.Data.Entities;
using VariationMicroservice.Service.DTOs;

namespace VariationMicroservice.Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Variation, VariationDto>();
            CreateMap<CreateVariationDto, Variation>();
            CreateMap<UpdateVariationDto, Variation>();
        }
    }
}