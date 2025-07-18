using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace OrderSagaOrchestrator.Data;

public class OrderSagaDbContext : DbContext
{
    public OrderSagaDbContext(DbContextOptions<OrderSagaDbContext> options) : base(options)
    {
    }

    public DbSet<SagaState> SagaStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the saga state
        modelBuilder.Entity<SagaState>(entity =>
        {
            entity.HasKey(e => e.CorrelationId);
            entity.Property(e => e.CorrelationId).IsRequired();
            entity.Property(e => e.CartId).IsRequired();
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.CurrentState).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Version).IsRequired().IsConcurrencyToken();
            
            // Add indexes for better performance
            entity.HasIndex(e => e.CartId);
            entity.HasIndex(e => e.CurrentState);
        });

        // NO OUTBOX TABLES FOR NOW
    }
}