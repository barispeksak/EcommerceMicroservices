using Microsoft.EntityFrameworkCore;
// using PaymentTypeMicroservice.Models;
using PaymentTypeMicroservice.Entities;


namespace PaymentTypeMicroservice.Data.Repositories
{
    public class PaymentTypeRepository : IPaymentTypeRepository
    {
        private readonly PaymentDbContext _context;
        public PaymentTypeRepository(PaymentDbContext context) => _context = context;

        public async Task<IEnumerable<PaymentType>> GetAllAsync()
            => await _context.PaymentTypes.ToListAsync();

        public async Task<PaymentType?> GetByIdAsync(int id)
            => await _context.PaymentTypes.FindAsync(id);

        public async Task<PaymentType> AddAsync(PaymentType entity)
        {
            _context.PaymentTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }


        public async Task<bool> UpdateAsync(PaymentType PaymentType)
        {
            if (!_context.PaymentTypes.Any(x => x.Id == PaymentType.Id)) return false;

            _context.Entry(PaymentType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.PaymentTypes.FindAsync(id);
            if (entity == null) return false;

            _context.PaymentTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ExistsByNameAsync(string typeName)
        {
            return await _context.PaymentTypes.AnyAsync(x => x.Type == typeName);
        }
    }
}
