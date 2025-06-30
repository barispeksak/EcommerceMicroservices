using Microsoft.EntityFrameworkCore;
using ShippingTypeMicroservice.Entities;

namespace ShippingTypeMicroservice.Data
{
    public class ShippingDbContext : DbContext
    {
        public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }

        public DbSet<ShippingType> ShippingTypes => Set<ShippingType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShippingType>().ToTable("shipping_type");
        }
    }
}
