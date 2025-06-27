using AutoMapper;
using UserMicroservice.Data.Entities;
using UserMicroservice.Dtos;

namespace UserMicroservice.Service.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, User>();
        }
    }
}
