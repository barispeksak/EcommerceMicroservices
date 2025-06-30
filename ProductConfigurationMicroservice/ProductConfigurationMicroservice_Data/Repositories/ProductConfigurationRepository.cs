using Microsoft.EntityFrameworkCore;
using ProductConfigurationMicroservice_Data.Entities;

namespace ProductConfigurationMicroservice_Data.Repositories;

public class ProductConfigurationRepository : IProductConfigurationRepository
{
    private readonly ProductConfigurationDbContext _ctx;

    public ProductConfigurationRepository(ProductConfigurationDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<ProductConfiguration>> GetAllAsync(int? productItemId = null) =>
        productItemId is null
            ? await _ctx.ProductConfigurations
                        .AsNoTracking()
                        .ToListAsync()
            : await _ctx.ProductConfigurations
                        .AsNoTracking()
                        .Where(pc => pc.ProductItemId == productItemId)
                        .ToListAsync();

    public Task<ProductConfiguration?> GetByIdAsync(int id) =>
        _ctx.ProductConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.Id == id);

    public Task RemoveAsync(ProductConfiguration entity)
    {
        _ctx.ProductConfigurations.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveAsync() => _ctx.SaveChangesAsync();
}
