using VariationMicroservice.Data.Entities;

namespace VariationMicroservice.Data.Repositories
{
    public interface IVariationRepository
    {
        Task<IEnumerable<Variation>> GetAllAsync();
        Task<Variation?> GetByIdAsync(int id);
        Task<Variation> CreateAsync(Variation variation);
        Task UpdateAsync(Variation variation);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}