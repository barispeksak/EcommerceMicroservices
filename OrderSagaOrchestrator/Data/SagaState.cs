using MassTransit;
using System;

namespace OrderSagaOrchestrator.Data;

public class SagaState : SagaStateMachineInstance
{
    // Primary key for saga correlation
    public Guid CorrelationId { get; set; }
    
    // Current state name
    public string CurrentState { get; set; } = "Initial";
    
    // Business properties
    public Guid CartId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // NEW: Payment timeout token for MassTransit scheduling
    public Guid? PaymentTimeoutToken { get; set; }
    
    // Version for optimistic concurrency - initialize here and in constructor
    public int Version { get; set; } = 1;
    
    // Constructor to ensure Version is always initialized
    public SagaState()
    {
        CurrentState = "Initial";
        CreatedAt = DateTime.UtcNow;
        Version = 1;
    }
}