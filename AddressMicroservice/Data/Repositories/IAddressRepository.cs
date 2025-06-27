using AddressMicroservice.Data.Entities;

namespace AddressMicroservice.Data.Repositories
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllAsync();
        Task<Address?> GetByIdAsync(int id);
        Task<Address> AddAsync(Address address);  // This should return Task<Address>
        Task UpdateAsync(Address address);
        Task DeleteAsync(Address address);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();
    }
}