using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Models;

// Use an alias to avoid ambiguity
using EntityShopOrder = ShopOrderMicroservice.Data.Entities.ShopOrder;

namespace ShopOrderMicroservice.Data.Repositories
{
    public class ShopOrderRepository : IShopOrderRepository
    {
        private readonly ShopOrderDbContext _context;

        public ShopOrderRepository(ShopOrderDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EntityShopOrder>> GetAllAsync()
            => await _context.ShopOrders.ToListAsync();

        public async Task<EntityShopOrder?> GetByIdAsync(int id)
            => await _context.ShopOrders.FindAsync(id);

        public async Task AddAsync(EntityShopOrder order)
        {
            _context.ShopOrders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(EntityShopOrder updatedOrder)
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

        public async Task<IEnumerable<EntityShopOrder>> GetByUserIdAsync(int userId)
            => await _context.ShopOrders
                            .Where(o => o.UserId == userId)
                            .ToListAsync();

        public async Task<IEnumerable<EntityShopOrder>> GetByDateRangeAsync(DateTime start, DateTime end)
            => await _context.ShopOrders
                            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                            .ToListAsync();

    }
}