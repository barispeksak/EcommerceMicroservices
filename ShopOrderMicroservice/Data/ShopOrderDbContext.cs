// ShopOrderMicroservice/Data/ShopOrderDbContext.cs
using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Data
{
    /// <summary>
    /// Uygulamadaki tüm tabloları yöneten DbContext.
    /// </summary>
    public sealed class ShopOrderDbContext : DbContext
    {
        public ShopOrderDbContext(DbContextOptions<ShopOrderDbContext> options)
            : base(options) { }

        // === DbSet’ler =====================================================
        public DbSet<ShopOrder>    ShopOrders     { get; set; }          // get-set  
        public DbSet<OrderStatus>  OrderStatuses  { get; set; }          // get-set  
        public DbSet<OrderSummary> OrderSummaries { get; set; }          // get-set  

        public DbSet<ShippingType> ShippingTypes  => Set<ShippingType>(); // readonly
        public DbSet<PaymentType>  PaymentTypes   => Set<PaymentType>();  // readonly

        // === Model yapılandırması =========================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tablo adları
            modelBuilder.Entity<ShopOrder>().ToTable("ShopOrder");
            modelBuilder.Entity<ShippingType>().ToTable("ShippingType");
            modelBuilder.Entity<OrderStatus>().ToTable("OrderStatus");
            modelBuilder.Entity<OrderSummary>().ToTable("OrderSummary");
            modelBuilder.Entity<PaymentType>().ToTable("PaymentType");


            modelBuilder.Entity<OrderStatus>()
                .HasIndex(s => s.Status)
                .IsUnique();
        }
    }
}
