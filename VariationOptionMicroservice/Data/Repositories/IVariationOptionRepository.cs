using VariationOptionMicroservice.Data.Entities;

namespace VariationOptionMicroservice.Data.Repositories
{
    public interface IVariationOptionRepository
    {
        Task<IEnumerable<VariationOption>> GetAllAsync();
        Task<VariationOption?> GetByIdAsync(int id);
        Task AddAsync(VariationOption variationOption);
        Task DeleteAsync(VariationOption variationOption);
        Task SaveAsync();
    }
}