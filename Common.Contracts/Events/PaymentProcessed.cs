using Common.Contracts.Base;

namespace Common.Contracts.Events
{
    public record PaymentProcessed : Base.Message
    {
        public Guid PaymentId { get; init; }
        public Guid CartId { get; init; }
        public Guid OrderId { get; init; }
        public decimal Amount { get; init; }
        public string TransactionId { get; init; } = string.Empty;
    }
}