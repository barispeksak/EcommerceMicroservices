using Microsoft.EntityFrameworkCore;
// using PaymentTypeMicroservice.Models;
using PaymentTypeMicroservice.Entities;


public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<PaymentType> PaymentTypes => Set<PaymentType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentType>().ToTable("payment_type");
    }
}

