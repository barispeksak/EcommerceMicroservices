using ProductCategoryMicroservice_Data.Entities;

namespace ProductCategoryMicroservice_Data.Repositories;

/// <summary>
/// product_category tablosu için veri erişim sözleşmesi
/// (CRUD + isteğe bağlı yardımcı sorgular).
/// </summary>
public interface ICategoryRepository
{
    Task<bool> AnyAsync(string categoryName, int? parentCategoryId);

    Task<Category?>             GetAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task AddAsync(Category entity);
    Task UpdateAsync(int id, Category updated);
    Task DeleteAsync(int id);
    Task SaveAsync();
}
