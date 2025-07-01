using Microsoft.EntityFrameworkCore;
using Variation_OptionMicroservice.Data.Entities;

namespace Variation_OptionMicroservice.Data
{
    public class Variation_OptionDbContext : DbContext
    {
        public Variation_OptionDbContext(DbContextOptions<Variation_OptionDbContext> options) : base(options) { }

        public DbSet<VariationOption> VariationOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VariationOption>()
                .Property(v => v.VariationId)
                .IsRequired();

        }
    }
}