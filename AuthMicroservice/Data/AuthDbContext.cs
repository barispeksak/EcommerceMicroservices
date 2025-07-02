using Microsoft.EntityFrameworkCore;
using AuthMicroservice.Data.Entities;

namespace AuthMicroservice.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>().ToTable("user-auth");

            // Gerekirse başka konfigürasyonlar da buraya eklenir
        }
    }
}
