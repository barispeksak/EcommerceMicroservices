using AutoMapper;
using VariationOptionMicroservice.Data.Entities;
using VariationOptionMicroservice.Service.DTOs;

namespace VariationOptionMicroservice.Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<VariationOption, VariationOptionDto>();
            CreateMap<CreateVariationOptionDto, VariationOption>();
            CreateMap<UpdateVariationOptionDto, VariationOption>();
        }
    }
}