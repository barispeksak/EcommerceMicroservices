namespace Common.Contracts.Events;

public record OrderSummaryCreated : Base.Message
{
    public Guid CartId { get; init; }
}