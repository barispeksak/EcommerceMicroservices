using Microsoft.EntityFrameworkCore;
using ProductMicroservice_Data.Entities;

namespace ProductMicroservice_Data;

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public ProductDbContext(DbContextOptions<ProductDbContext> opt) : base(opt) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("product");
        b.Entity<Product>(e =>
        {
            e.ToTable("product"); // tablo adı

            e.HasKey(p => p.Id);


            e.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            e.Property(p => p.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(100);

            e.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            e.Property(p => p.Image)
                .HasColumnName("image")
                .HasMaxLength(250);

            e.Property(p => p.Brand)
                .HasColumnName("brand")
                .HasMaxLength(100);
                
            e.Property(p => p.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

        });
    }
}
