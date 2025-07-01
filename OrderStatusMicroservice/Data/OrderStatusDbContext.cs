using Microsoft.EntityFrameworkCore;
using OrderStatusMicroservice.Entities;

namespace OrderStatusMicroservice.Data
{
    public class OrderStatusDbContext : DbContext
    {
        public OrderStatusDbContext(DbContextOptions<OrderStatusDbContext> options)
            : base(options)
        {
        }

        // DbSet
        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderStatus>().ToTable("Order_status");

            // Opsiyonel: Fluent API örneği
            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(100);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShopOrderId).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
