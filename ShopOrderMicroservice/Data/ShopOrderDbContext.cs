using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Data
{
    public class ShopOrderDbContext : DbContext
    {
        public ShopOrderDbContext(DbContextOptions<ShopOrderDbContext> options) : base(options) { }

        // Tablolar
        public DbSet<ShopOrder> ShopOrders => Set<ShopOrder>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ShopOrder tablosu yapılandırması
            modelBuilder.Entity<ShopOrder>(entity =>
            {
                entity.ToTable("shop_order");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.OrderDate).HasColumnName("order_date");
                entity.Property(x => x.PaymentTypeId).HasColumnName("payment_type_id");
                entity.Property(x => x.ShippingAddressId).HasColumnName("shipping_address_id");
                entity.Property(x => x.ShippingTypeId).HasColumnName("shipping_type_id");
            });
        }
    }
}
