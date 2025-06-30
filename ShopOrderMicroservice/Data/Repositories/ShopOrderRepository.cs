using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopOrderMicroservice.Repositories
{
    public class ShopOrderRepository : IShopOrderRepository
    {
        private readonly ShopOrderDbContext _context;

        public ShopOrderRepository(ShopOrderDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShopOrder>> GetAllAsync()
            => await _context.ShopOrders.ToListAsync();

        public async Task<ShopOrder?> GetByIdAsync(int id)
            => await _context.ShopOrders.FindAsync(id);

        public async Task AddAsync(ShopOrder order)
        {
            _context.ShopOrders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(ShopOrder updatedOrder)
        {
            _context.ShopOrders.Update(updatedOrder);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.ShopOrders.FindAsync(id);
            if (order == null) return false;

            _context.ShopOrders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        //  Kullanıcıya göre siparişler
        public async Task<IEnumerable<ShopOrder>> GetByUserIdAsync(int userId)
            => await _context.ShopOrders
                             .Where(o => o.UserId == userId)
                             .ToListAsync();

        //  Tarih aralığına göre siparişler
        public async Task<IEnumerable<ShopOrder>> GetByDateRangeAsync(DateTime start, DateTime end)
            => await _context.ShopOrders
                             .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                             .ToListAsync();

    }
}
