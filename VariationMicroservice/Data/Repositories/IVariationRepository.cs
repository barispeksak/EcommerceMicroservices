using VariationMicroservice.Data.Entities;

namespace VariationMicroservice.Data.Repositories
{
    public interface IVariationRepository
{
    Task<IEnumerable<Variation>> GetAllAsync();
    Task<Variation?> GetByIdAsync(int id);
    Task AddAsync(Variation variation);
    Task DeleteAsync(Variation variation);
    Task SaveAsync();
}
}