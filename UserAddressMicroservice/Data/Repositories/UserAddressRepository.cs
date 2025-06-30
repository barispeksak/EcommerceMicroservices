// 2. Repository Implementation - UserAddressRepository.cs
using Microsoft.EntityFrameworkCore;
using UserAddressMicroservice.Data.Entities;

namespace UserAddressMicroservice.Data.Repositories
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly UserAddressDbContext _context;

        public UserAddressRepository(UserAddressDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserAddress>> GetAllAsync()
            => await _context.UserAddresses.ToListAsync();

        public async Task<UserAddress?> GetAsync(int userId, int addressId)
            => await _context.UserAddresses.FindAsync(userId, addressId);

        public async Task AddAsync(UserAddress entity)
        {
            _context.UserAddresses.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int userId, int addressId)
        {
            var entity = await _context.UserAddresses.FindAsync(userId, addressId);
            if (entity == null) return false;

            _context.UserAddresses.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
