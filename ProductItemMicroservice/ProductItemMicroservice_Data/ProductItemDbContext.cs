using Microsoft.EntityFrameworkCore;
using ProductItemMicroservice_Data.Entities;

namespace ProductItemMicroservice_Data
{
    public class ProductItemDbContext : DbContext
    {
        public DbSet<ProductItem> ProductItems => Set<ProductItem>();

        public ProductItemDbContext(DbContextOptions<ProductItemDbContext> opt)
            : base(opt) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.HasDefaultSchema("item");

            b.Entity<ProductItem>(e =>
            {
                e.ToTable("product_item");          // tablo adı

                e.HasKey(p => p.Id);

                e.Property(p => p.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();         // SERIAL / identity

                e.Property(p => p.Sku)
                    .HasColumnName("sku")
                    .IsRequired()
                    .HasMaxLength(30);

                e.Property(p => p.QuantityInStock)
                    .HasColumnName("quantity_in_stock")
                    .IsRequired();

                e.Property(p => p.Price)
                    .HasColumnName("price")
                    .IsRequired();

                e.Property(p => p.Currency)
                    .HasColumnName("currency")
                    .IsRequired()
                    .HasMaxLength(3);

                e.Property(p => p.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                /* İndex — benzersiz SKU için */
                e.HasIndex(p => p.Sku).IsUnique();
            });
        }
    }
}
