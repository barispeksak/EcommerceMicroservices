using Microsoft.EntityFrameworkCore;
using PaymentTypeMicroservice.Models;
using EntityPaymentType = PaymentTypeMicroservice.Entities.PaymentType; // Use alias to avoid ambiguity

namespace PaymentTypeMicroservice.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        public DbSet<EntityPaymentType> PaymentTypes => Set<EntityPaymentType>(); // Use alias
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityPaymentType>().ToTable("payment_type"); // Use alias
            
            // Add Payment entity configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Status).HasMaxLength(50);
                entity.Property(x => x.TransactionId).HasMaxLength(100);
                entity.Property(x => x.Amount).HasPrecision(18, 2);
            });
        }
    }
}