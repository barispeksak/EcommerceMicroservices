using Microsoft.EntityFrameworkCore;
using AddressMicroservice.Data.Entities;

namespace AddressMicroservice.Data.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AddressDbContext _context;

        public AddressRepository(AddressDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetAllAsync()
        {
            return await _context.Addresses
                .OrderBy(a => a.Id)
                .ToListAsync();
        }

        public async Task<Address?> GetByIdAsync(int id)
        {
            return await _context.Addresses.FindAsync(id);
        }

        public async Task<Address> AddAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
            return address;  // Return the address
        }

        public Task UpdateAsync(Address address)
        {
            _context.Entry(address).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Address address)
        {
            _context.Addresses.Remove(address);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Addresses.AnyAsync(a => a.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}