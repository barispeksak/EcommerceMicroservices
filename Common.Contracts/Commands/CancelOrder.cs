namespace Common.Contracts.Commands;

public record CancelOrder : Base.Message
{
    public CancelOrder(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; init; }
}