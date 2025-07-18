namespace Common.Contracts.Events;

public record StockReserved : Base.Message
{
    public Guid CartId { get; init; }
}