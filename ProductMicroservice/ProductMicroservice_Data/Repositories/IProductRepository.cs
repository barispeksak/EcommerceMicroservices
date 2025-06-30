using ProductMicroservice.Data.Entities;

namespace ProductMicroservice.Data.Repositories;

/// <summary>
/// Product tablosu için veri erişim sözleşmesi
/// (tüm CRUD operasyonlarını içerir).
/// </summary>
public interface IProductRepository
{
    Task<Product?>             GetAsync(int id);          // GET /{id}
    Task<IEnumerable<Product>> GetAllAsync();             // GET /
    Task AddAsync(Product entity);                        // POST
    Task DeleteAsync(Product entity);                     // DELETE
    Task SaveAsync();                                     // SaveChanges
}
