// 3. DbContext - UserAddressDbContext.cs
using Microsoft.EntityFrameworkCore;
using UserAddressMicroservice.Data.Entities;

namespace UserAddressMicroservice.Data
{
    public class UserAddressDbContext : DbContext
    {
        public UserAddressDbContext(DbContextOptions<UserAddressDbContext> options) : base(options) { }

        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAddress>().HasKey(x => new { x.UserId, x.AddressId });
        }
    }
}