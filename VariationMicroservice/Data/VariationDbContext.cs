using Microsoft.EntityFrameworkCore;
using VariationMicroservice.Data.Entities;

namespace VariationMicroservice.Data
{
    public class VariationDbContext : DbContext
    {
        public VariationDbContext(DbContextOptions<VariationDbContext> options) : base(options) { }

        public DbSet<Variation> Variations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Eğer Variation entity'sinde CategoryId varsa, zorunlu yapıyoruz
            modelBuilder.Entity<Variation>()
                .Property(v => v.CategoryId)
                .IsRequired();
        }
    }
}