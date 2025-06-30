using Microsoft.EntityFrameworkCore;
using ShippingTypeMicroservice.Models;

namespace ShippingTypeMicroservice.Data.Repositories
{
    public class ShippingTypeRepository : IShippingTypeRepository
    {
        private readonly ShippingDbContext _context;
        public ShippingTypeRepository(ShippingDbContext context) => _context = context;

        public async Task<IEnumerable<ShippingType>> GetAllAsync()
            => await _context.ShippingTypes.ToListAsync();

        public async Task<ShippingType?> GetByIdAsync(int id)
            => await _context.ShippingTypes.FindAsync(id);

        public async Task<ShippingType> AddAsync(ShippingType entity)
        {
            _context.ShippingTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }


        public async Task<bool> UpdateAsync(ShippingType shippingType)
        {
            if (!_context.ShippingTypes.Any(x => x.Id == shippingType.Id)) return false;

            _context.Entry(shippingType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ShippingTypes.FindAsync(id);
            if (entity == null) return false;

            _context.ShippingTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
