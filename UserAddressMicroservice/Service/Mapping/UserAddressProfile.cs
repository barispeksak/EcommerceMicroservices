using AutoMapper;
using UserAddressMicroservice.Data.Entities;
using UserAddressMicroservice.Data.Dtos;

namespace UserAddressMicroservice.Service.Mapping
{
        public class UserAddressProfile : Profile
    {
        public UserAddressProfile()
        {
            CreateMap<UserAddress, UserAddressDto>().ReverseMap();
            CreateMap<CreateUserAddressDto, UserAddress>();
        }
    }

}
