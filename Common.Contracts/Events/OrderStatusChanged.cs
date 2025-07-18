namespace Common.Contracts.Events;
public record OrderStatusChanged : Base.Message
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = default!;
}