using Microsoft.EntityFrameworkCore;
using AddressMicroservice.Data.Entities;

namespace AddressMicroservice.Data
{
    public class AddressDbContext : DbContext
    {
        public AddressDbContext(DbContextOptions<AddressDbContext> options) : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.AddressLine).IsRequired().HasMaxLength(500);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);  
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                
                entity.ToTable("Addresses");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}