using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Models;
using EntityShopOrder = ShopOrderMicroservice.Data.Entities.ShopOrder;

namespace ShopOrderMicroservice.Data
{
    public class ShopOrderDbContext : DbContext
    {
        public ShopOrderDbContext(DbContextOptions<ShopOrderDbContext> options) : base(options) { }

        public DbSet<EntityShopOrder> ShopOrders => Set<EntityShopOrder>();
        public DbSet<Order> Orders => Set<Order>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityShopOrder>(entity =>
            {
                entity.ToTable("shop_order");
                
                entity.HasKey(x => x.Id);
                
                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.OrderDate).HasColumnName("order_date");
                entity.Property(x => x.PaymentTypeId).HasColumnName("payment_type_id");
                entity.Property(x => x.ShippingAddressId).HasColumnName("shipping_address_id");
                entity.Property(x => x.ShippingTypeId).HasColumnName("shipping_type_id");
                entity.Property(x => x.ShopId).HasColumnName("shopping_cart_id");
                entity.Property(x => x.OrderTotal).HasColumnName("total_price");
            });
            
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("quick_orders");
            });
        }
    }
}