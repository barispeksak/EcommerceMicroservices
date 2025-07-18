namespace Common.Contracts.Events;

public record OrderFailed : Base.Message
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = default!;
    
    public Guid CartId { get; set; }
}