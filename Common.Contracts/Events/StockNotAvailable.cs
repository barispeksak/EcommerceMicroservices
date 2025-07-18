namespace Common.Contracts.Events;
public record StockNotAvailable : Base.Message
{
    public Guid CartId { get; init; }
}