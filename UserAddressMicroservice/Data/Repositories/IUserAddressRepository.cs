// 1. Repository Interface
using UserAddressMicroservice.Data.Entities;

namespace UserAddressMicroservice.Data.Repositories
{
    public interface IUserAddressRepository
    {
        Task<IEnumerable<UserAddress>> GetAllAsync();
        Task<UserAddress?> GetAsync(int userId, int addressId);
        Task AddAsync(UserAddress entity);
        Task<bool> DeleteAsync(int userId, int addressId);
    }
}