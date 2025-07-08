using Microsoft.EntityFrameworkCore;
using VariationOptionMicroservice.Data.Entities;

namespace VariationOptionMicroservice.Data.Repositories
{
    public class VariationOptionRepository : IVariationOptionRepository
    {
        private readonly VariationOptionDbContext _context;

        public VariationOptionRepository(VariationOptionDbContext context)
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