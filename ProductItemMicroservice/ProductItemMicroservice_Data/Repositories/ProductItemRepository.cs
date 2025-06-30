using Microsoft.EntityFrameworkCore;
using ProductItemMicroservice_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductItemMicroservice_Data.Repositories
{
    public class ProductItemRepository : IProductItemRepository
    {
        private readonly ProductItemDbContext _context;

        public ProductItemRepository(ProductItemDbContext context)
        {
            _context = context;
        }

        public async Task<ProductItem> GetByIdAsync(int id)
        {
            return await _context.ProductItems.FindAsync(id);
        }

        public async Task<List<ProductItem>> GetAllAsync()
        {
            return await _context.ProductItems.ToListAsync();
        }

        public async Task<ProductItem> GetBySkuAsync(string sku)
        {
            return await _context.ProductItems.FirstOrDefaultAsync(x => x.Sku == sku);
        }

        public async Task AddAsync(ProductItem entity)
        {
            await _context.ProductItems.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductItem entity)
        {
            _context.ProductItems.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ProductItem entity)
        {
            _context.ProductItems.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SkuExistsAsync(string sku)
        {
            return await _context.ProductItems.AnyAsync(x => x.Sku == sku);
        }
    }
}
