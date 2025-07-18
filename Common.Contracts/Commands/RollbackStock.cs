namespace Common.Contracts.Commands;

public record RollbackStock : Base.Message
{
    public RollbackStock(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; init; }
}