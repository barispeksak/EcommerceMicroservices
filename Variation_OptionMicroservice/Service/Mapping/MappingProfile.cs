using AutoMapper;
using Variation_OptionMicroservice.Data.Entities;
using Variation_OptionMicroservice.Service.DTOs;

namespace Variation_OptionMicroservice.Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<VariationOption, VariationOptionDto>()
                .ForMember(dest => dest.VariationName, 
                    opt => opt.MapFrom(src => src.Variation != null ? src.Variation.VarTypeName : null));
            
            CreateMap<CreateVariationOptionDto, VariationOption>();
            CreateMap<UpdateVariationOptionDto, VariationOption>();
        }
    }
}