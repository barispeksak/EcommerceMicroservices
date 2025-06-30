using Microsoft.EntityFrameworkCore;
using ProductCategoryMicroservice_Data.Entities;

namespace ProductCategoryMicroservice_Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    
    private readonly CategoryDbContext _context;

    public CategoryRepository(CategoryDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AnyAsync(string categoryName, int? parentCategoryId)
    {
        if (parentCategoryId == null)
        {
            // Ana kategori kontrolü
            return await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == null && c.CategoryName == categoryName);
        }
        else
        {
            // Alt kategori kontrolü
            return await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == parentCategoryId && c.CategoryName == categoryName);
        }
    }

    public Task<Category?> GetAsync(int id) =>
        _context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Category>> GetAllAsync() =>
        await _context.Categories
                  .AsNoTracking()
                  .OrderBy(c => c.CategoryName)
                  .ToListAsync();

    public async Task AddAsync(Category entity)
    {
        await _context.Categories.AddAsync(entity);
    }

    public async Task UpdateAsync(int id, Category updated)
    {
        var entity = await _context.Categories.FindAsync(id);
        if (entity is null) return;

        entity.CategoryName = updated.CategoryName;
        entity.ParentCategoryId = updated.ParentCategoryId;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Categories.FindAsync(id);
        if (entity is null) return;
        _context.Categories.Remove(entity);
    }

    public Task SaveAsync() => _context.SaveChangesAsync();
    
}
