using Microsoft.EntityFrameworkCore;
using VariationMicroservice.Data.Entities;

namespace VariationMicroservice.Data
{
    public class VariationDbContext : DbContext
    {
        public VariationDbContext(DbContextOptions<VariationDbContext> options) : base(options) { }

        public DbSet<Variation> Variations { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<VariationOption> VariationOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Variation - Category ilişkisi
            modelBuilder.Entity<Variation>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Variations)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Variation - Options ilişkisi
            modelBuilder.Entity<VariationOption>()
                .HasOne(o => o.Variation)
                .WithMany(v => v.Options)
                .HasForeignKey(o => o.VariationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}