using AutoMapper;
using UserAddressMicroservice.Data.Entities;
using UserAddressMicroservice.Data.Repositories;
using UserAddressMicroservice.Data.Dtos;
using UserAddressMicroservice.Service.Interfaces;

namespace UserAddressMicroservice.Service.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IUserAddressRepository _repo;
        private readonly IMapper _mapper;

        public UserAddressService(IUserAddressRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserAddressDto>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserAddressDto>>(data);
        }

        public async Task<UserAddressDto> GetAsync(int userId, int addressId)
        {
            var data = await _repo.GetAsync(userId, addressId);
            return _mapper.Map<UserAddressDto>(data);
        }

        public async Task<UserAddressDto> CreateAsync(CreateUserAddressDto dto)
        {
            var entity = _mapper.Map<UserAddress>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<UserAddressDto>(entity);
        }

        public async Task<bool> DeleteAsync(int userId, int addressId)
            => await _repo.DeleteAsync(userId, addressId);
    }

}
