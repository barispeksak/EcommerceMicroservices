using UserAddressMicroservice.Data.Dtos;

namespace UserAddressMicroservice.Service.Interfaces
{
    public interface IUserAddressService
{
    Task<IEnumerable<UserAddressDto>> GetAllAsync();
    Task<UserAddressDto> GetAsync(int userId, int addressId);
    Task<UserAddressDto> CreateAsync(CreateUserAddressDto dto);
    Task<bool> DeleteAsync(int userId, int addressId);
}

}
