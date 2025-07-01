using Microsoft.EntityFrameworkCore;
using ShoppingCartMicroservice_Data;
using ShoppingCartMicroservice_Data.Entities;
using ShoppingCartMicroservice_Service.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartMicroservice_Data.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ShoppingCartDbContext _dbContext;

        public ShoppingCartRepository(ShoppingCartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ShoppingCart> AddAsync(ShoppingCart entity)
        {
            await _dbContext.ShoppingCarts.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbContext.ShoppingCarts.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<ShoppingCart> GetByIdAsync(int id)
        {
            return await _dbContext.ShoppingCarts.FindAsync(id);
        }

        public async Task<List<ShoppingCart>> GetByCartIdAsync(int cartId)
        {
            return await _dbContext.ShoppingCarts
                .Where(x => x.CartId == cartId)                //  ✅  CartId filtresi
                .ToListAsync();
        }

        public async Task<ShoppingCart> GetTotalRowByCartIdAsync(int cartId)
        {
            return await _dbContext.ShoppingCarts
                .FirstOrDefaultAsync(x => x.CartId == cartId && x.IsTotalRow);
        }

        public async Task UpdateAsync(ShoppingCart entity)
        {
            _dbContext.ShoppingCarts.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
