using Microsoft.EntityFrameworkCore;
using Variation_OptionMicroservice.Data.Entities;

namespace Variation_OptionMicroservice.Data
{
    public class Variation_OptionDbContext : DbContext
    {
        public Variation_OptionDbContext(DbContextOptions<Variation_OptionDbContext> options) : base(options) { }

        public DbSet<VariationOption> VariationOptions { get; set; }
        public DbSet<Variation> Variations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // VariationOption - Variation ilişkisi
            modelBuilder.Entity<VariationOption>()
                .HasOne(vo => vo.Variation)
                .WithMany(v => v.Options)
                .HasForeignKey(vo => vo.VariationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index ekle
            modelBuilder.Entity<VariationOption>()
                .HasIndex(vo => vo.VariationId);
        }
    }
}