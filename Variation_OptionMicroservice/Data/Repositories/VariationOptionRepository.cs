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
            return await _context.VariationOptions
                .Include(vo => vo.Variation)
                .ToListAsync();
        }

        public async Task<VariationOption?> GetByIdAsync(int id)
        {
            return await _context.VariationOptions
                .Include(vo => vo.Variation)
                .FirstOrDefaultAsync(vo => vo.Id == id);
        }

        public async Task<IEnumerable<VariationOption>> GetByVariationIdAsync(int variationId)
        {
            return await _context.VariationOptions
                .Where(vo => vo.VariationId == variationId)
                .Include(vo => vo.Variation)
                .ToListAsync();
        }

        public async Task<VariationOption> CreateAsync(VariationOption variationOption)
        {
            _context.VariationOptions.Add(variationOption);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(variationOption.Id) ?? variationOption;
        }

        public async Task UpdateAsync(VariationOption variationOption)
        {
            _context.Entry(variationOption).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var variationOption = await _context.VariationOptions.FindAsync(id);
            if (variationOption != null)
            {
                _context.VariationOptions.Remove(variationOption);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.VariationOptions.AnyAsync(vo => vo.Id == id);
        }

        public async Task<bool> VariationExistsAsync(int variationId)
        {
            return await _context.Variations.AnyAsync(v => v.Id == variationId);
        }
    }
}