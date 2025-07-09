using Microsoft.EntityFrameworkCore;
using ProductMicroservice_Data.Entities;

namespace ProductMicroservice_Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _ctx;
    public ProductRepository(ProductDbContext ctx) => _ctx = ctx;

    public Task<Product?> GetAsync(int id) =>
        _ctx.Products.FindAsync(id).AsTask();

    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await _ctx.Products.AsNoTracking().ToListAsync();

    public Task AddAsync(Product entity) =>
        _ctx.Products.AddAsync(entity).AsTask();

    public Task DeleteAsync(Product entity)
    {
        _ctx.Products.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveAsync() => _ctx.SaveChangesAsync();
}
