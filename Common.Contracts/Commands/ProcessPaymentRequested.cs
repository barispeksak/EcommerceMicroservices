using Common.Contracts.Base;

namespace Common.Contracts.Commands
{
    public record ProcessPaymentRequested : Base.Message
    {
        public Guid CartId { get; init; }
        public Guid OrderId { get; init; }
        public decimal Amount { get; init; }
        public int PaymentTypeId { get; init; }
    }
}