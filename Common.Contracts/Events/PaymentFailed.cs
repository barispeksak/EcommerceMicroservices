using Common.Contracts.Base;

namespace Common.Contracts.Events
{
    public record PaymentFailed : Base.Message
    {
        public Guid CartId { get; init; }
        public Guid OrderId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}