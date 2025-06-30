// ProductConfigurationMicroservice_Data/ProductConfigurationDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProductConfigurationMicroservice_Data.Entities;

namespace ProductConfigurationMicroservice_Data;

public class ProductConfigurationDbContext : DbContext
{
    public ProductConfigurationDbContext(DbContextOptions<ProductConfigurationDbContext> opts)
        : base(opts) { }

    public DbSet<ProductConfiguration> ProductConfigurations => Set<ProductConfiguration>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ProductConfiguration>(cfg =>
        {
            // 🏷️ Tablo adını biz belirliyoruz
            cfg.ToTable("product_configuration");         

            cfg.HasKey(x => x.Id);

            // ⚠️ FK yok – scalar kolonlar
            cfg.Property(x => x.ProductItemId)
               .HasColumnName("product_item_id")
               .IsRequired();

            cfg.Property(x => x.VariationOptionId)
               .HasColumnName("variation_option_id")
               .IsRequired();

            // 🔒 Aynı SKU-Option ikilisi ikinci kez eklenemesin
            cfg.HasIndex(x => new { x.ProductItemId, x.VariationOptionId })
               .IsUnique();
        });
    }
}
