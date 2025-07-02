using AutoMapper;
using AuthMicroservice.Data.Entities;
using AuthMicroservice.Service.DTOs;

namespace AuthMicroservice.Service.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            // RegisterDto → User (entity) dönüşümü
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName != null ? src.FirstName.Trim() : null))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName != null ? src.LastName.Trim() : null))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Password hash işlemine AutoMapper karışmaz
        }
    }
}
