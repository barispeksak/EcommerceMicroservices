using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Data
{
    public class ShopOrderDbContext : DbContext
    {
        public ShopOrderDbContext(DbContextOptions<ShopOrderDbContext> options) : base(options)
        {
        }

        public DbSet<ShopOrder> ShopOrders { get; set; }
        public DbSet<ShippingType> ShippingTypes { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShopOrder>().ToTable("ShopOrder");
            modelBuilder.Entity<ShippingType>().ToTable("ShippingType");
            modelBuilder.Entity<OrderStatus>().ToTable("OrderStatus");

            modelBuilder.Entity<ShopOrder>()
                .HasOne(s => s.ShippingType)
                .WithMany()
                .HasForeignKey(s => s.ShippingTypeId);

            modelBuilder.Entity<ShopOrder>()
                .HasOne(s => s.OrderStatus)
                .WithMany()
                .HasForeignKey(s => s.OrderStatusId);
        }
    }
}
