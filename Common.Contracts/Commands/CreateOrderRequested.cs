namespace Common.Contracts.Commands;

public record CreateOrderRequested : Base.Message
{
    public CreateOrderRequested(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; init; }
}