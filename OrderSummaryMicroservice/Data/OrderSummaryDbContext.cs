using Microsoft.EntityFrameworkCore;
using OrderSummaryMicroservice.Models;

namespace OrderSummaryMicroservice.Data
{
    public class OrderSummaryDbContext : DbContext
    {
        public OrderSummaryDbContext(DbContextOptions<OrderSummaryDbContext> options) : base(options)
        {
        }

        public DbSet<OrderSummary> OrderSummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderSummary>().ToTable("OrderSummary");
        }
    }
}