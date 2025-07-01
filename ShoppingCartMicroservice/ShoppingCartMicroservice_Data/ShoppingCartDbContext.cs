using Microsoft.EntityFrameworkCore;
using ShoppingCartMicroservice_Data.Entities;

namespace ShoppingCartMicroservice_Data
{
    public class ShoppingCartDbContext : DbContext
    {
        public ShoppingCartDbContext(DbContextOptions<ShoppingCartDbContext> options)
            : base(options)
        {
        }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.ToTable("shopping_cart");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.ProductItemId)
                    .HasColumnName("product_item_id")
                    .IsRequired();

                entity.Property(e => e.Qty)
                    .HasColumnName("qty")
                    .IsRequired();

                entity.Property(e => e.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.LinePrice)
                    .HasColumnName("line_price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.IsTotalRow)
                    .HasColumnName("is_total_row")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(e => e.TotalPrice)
                    .HasColumnName("total_price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });
        }
    }
}
