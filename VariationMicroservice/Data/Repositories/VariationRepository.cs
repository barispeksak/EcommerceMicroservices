using Microsoft.EntityFrameworkCore;
using VariationMicroservice.Data.Entities;

namespace VariationMicroservice.Data.Repositories
{
    public class VariationRepository : IVariationRepository
    {
        private readonly VariationDbContext _context;

        public VariationRepository(VariationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Variation>> GetAllAsync()
        {
            return await _context.Variations
                .Include(v => v.Category)
                .Include(v => v.Options)
                .ToListAsync();
        }

        public async Task<Variation?> GetByIdAsync(int id)
        {
            return await _context.Variations
                .Include(v => v.Category)
                .Include(v => v.Options)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Variation> CreateAsync(Variation variation)
        {
            _context.Variations.Add(variation);
            await _context.SaveChangesAsync();
            
            // Yeni oluşturulan varyasyonu ilişkileriyle birlikte döndür
            return await GetByIdAsync(variation.Id) ?? variation;
        }

        public async Task UpdateAsync(Variation variation)
        {
            _context.Entry(variation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var variation = await _context.Variations.FindAsync(id);
            if (variation != null)
            {
                _context.Variations.Remove(variation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Variations.AnyAsync(v => v.Id == id);
        }
    }
}