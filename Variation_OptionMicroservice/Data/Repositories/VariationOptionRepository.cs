using Microsoft.EntityFrameworkCore;
using Variation_OptionMicroservice.Data.Entities;

namespace Variation_OptionMicroservice.Data.Repositories
{
    public class VariationOptionRepository : IVariationOptionRepository
    {
        private readonly Variation_OptionDbContext _context;

        public VariationOptionRepository(Variation_OptionDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VariationOption>> GetAllAsync()
        {
            return await _context.VariationOptions.ToListAsync();
        }

        public async Task<VariationOption?> GetByIdAsync(int id)
        {
            return await _context.VariationOptions.FindAsync(id);
        }

        public async Task AddAsync(VariationOption variationOptions)
        {
            await _context.VariationOptions.AddAsync(variationOptions);
        }

        public Task DeleteAsync(VariationOption variationOption)
        {
            _context.VariationOptions.Remove(variationOption);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }



    }
}