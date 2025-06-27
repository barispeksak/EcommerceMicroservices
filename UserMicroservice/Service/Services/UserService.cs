using AutoMapper;
using UserMicroservice.Data.Entities;
using UserMicroservice.Data.Repositories;
using UserMicroservice.Dtos;
using UserMicroservice.Service.Interfaces;

namespace UserMicroservice.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            await _repo.AddAsync(user);
            return _mapper.Map<UserDto>(user);
        }


        public async Task<bool> UpdateUserAsync(UserDto userDto)
        {
            var existing = await _repo.GetByIdAsync(userDto.Id);
            if (existing is null) return false;
            _mapper.Map(userDto, existing);
            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user is null) return false;
            await _repo.DeleteAsync(user);
            return true;
        }
    }
}
