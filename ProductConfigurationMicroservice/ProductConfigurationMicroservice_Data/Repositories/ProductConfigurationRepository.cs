using Microsoft.EntityFrameworkCore;
using ProductConfigurationMicroservice_Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductConfigurationMicroservice_Data.Repositories
{
    public class ProductConfigurationRepository : IProductConfigurationRepository
    {
        private readonly ProductConfigurationDbContext _ctx;
        public ProductConfigurationRepository(ProductConfigurationDbContext ctx) => _ctx = ctx;

        /* ─────── LISTE METOTLARI ─────── */

        // Geriye uyumluluk için: tekli productItemId
        public async Task<IEnumerable<ProductConfiguration>> GetAllAsync(int? productItemId = null) =>
            productItemId is null
                ? await _ctx.ProductConfigurations.AsNoTracking().ToListAsync()
                : await _ctx.ProductConfigurations.AsNoTracking()
                        .Where(pc => pc.ProductItemId == productItemId)
                        .ToListAsync();

        // Yeni: çoklu filtre
        public async Task<IEnumerable<ProductConfiguration>> GetAllAsync(
            int[]? productItemIds,
            int[]? variationOptionIds)
        {
            var query = _ctx.ProductConfigurations.AsNoTracking().AsQueryable();

            if (productItemIds?.Any() == true)
                query = query.Where(pc => productItemIds.Contains(pc.ProductItemId));

            if (variationOptionIds?.Any() == true)
                query = query.Where(pc => variationOptionIds.Contains(pc.VariationOptionId));

            return await query.ToListAsync();
        }

        /* ─────── CRUD METOTLARI ─────── */

        public Task<ProductConfiguration?> GetByIdAsync(int id) =>
            _ctx.ProductConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(pc => pc.Id == id);

        public Task AddAsync(ProductConfiguration entity)
        {
            _ctx.ProductConfigurations.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ProductConfiguration entity)
        {
            _ctx.ProductConfigurations.Update(entity);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(ProductConfiguration entity)
        {
            _ctx.ProductConfigurations.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveAsync() => _ctx.SaveChangesAsync();
    }
}
