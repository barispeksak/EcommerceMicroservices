using Microsoft.EntityFrameworkCore;
using OrderStatusMicroservice.Entities;

namespace OrderStatusMicroservice.Data.Repositories
{
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly OrderStatusDbContext _context;
        public OrderStatusRepository(OrderStatusDbContext context) => _context = context;

        public async Task<IEnumerable<OrderStatus>> GetAllAsync()
            => await _context.OrderStatuses.ToListAsync();

        public async Task<OrderStatus?> GetByIdAsync(int id)
            => await _context.OrderStatuses.FindAsync(id);

        public async Task<OrderStatus> AddAsync(OrderStatus entity)
        {
            _context.OrderStatuses.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }


        public async Task<bool> UpdateAsync(OrderStatus OrderStatus)
        {
            if (!_context.OrderStatuses.Any(x => x.Id == OrderStatus.Id)) return false;

            _context.Entry(OrderStatus).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.OrderStatuses.FindAsync(id);
            if (entity == null) return false;

            _context.OrderStatuses.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
