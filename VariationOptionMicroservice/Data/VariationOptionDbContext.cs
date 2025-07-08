using Microsoft.EntityFrameworkCore;
using VariationOptionMicroservice.Data.Entities;

namespace VariationOptionMicroservice.Data
{
    public class VariationOptionDbContext : DbContext
    {
        public VariationOptionDbContext(DbContextOptions<VariationOptionDbContext> options) : base(options) { }

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