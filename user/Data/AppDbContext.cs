using Microsoft.EntityFrameworkCore;
using trendyolApi.Models;

namespace trendyolApi.Data{
    public class AppDbContext : DbContext{ // EF Core’un sağladığı DbContext sınıfından inherit aldık 
    
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){
        }

        public AppDbContext() : base(){
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<User>().ToTable("user"); // tablo adını net belirt
        }
    }
}
