namespace Common.Contracts.Events;

public record OrderCreated : Base.Message
{
    public Guid OrderId { get; init; }
    
    public Guid CartId { get; init; }
}