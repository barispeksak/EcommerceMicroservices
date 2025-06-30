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
            return await _context.Variations.ToListAsync();
        }

        public async Task<Variation?> GetByIdAsync(int id)
        {
            return await _context.Variations.FindAsync(id);
        }

        public async Task AddAsync(Variation variation)
        {
            await _context.Variations.AddAsync(variation);
        }

        public async Task DeleteAsync(Variation variation)
        {
            _context.Variations.Remove(variation);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}