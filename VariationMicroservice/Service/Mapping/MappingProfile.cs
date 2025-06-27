using AutoMapper;
using VariationMicroservice.Data.Entities;
using VariationMicroservice.Service.DTOs;

namespace VariationMicroservice.Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Variation, VariationDto>()
                .ForMember(dest => dest.CategoryName, 
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
            
            CreateMap<CreateVariationDto, Variation>();
            CreateMap<UpdateVariationDto, Variation>();
            CreateMap<VariationOption, VariationOptionDto>();
        }
    }
}