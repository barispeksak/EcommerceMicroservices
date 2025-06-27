using Variation_OptionMicroservice.Data.Entities;

namespace Variation_OptionMicroservice.Data.Repositories
{
    public interface IVariationOptionRepository
    {
        Task<IEnumerable<VariationOption>> GetAllAsync();
        Task<VariationOption?> GetByIdAsync(int id);
        Task<IEnumerable<VariationOption>> GetByVariationIdAsync(int variationId);
        Task<VariationOption> CreateAsync(VariationOption variationOption);
        Task UpdateAsync(VariationOption variationOption);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> VariationExistsAsync(int variationId);
    }
}