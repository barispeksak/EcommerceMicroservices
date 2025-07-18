using Common.Contracts.Base;
using System;

namespace Common.Contracts.Timeouts
{
    // Add 'record' keyword if Message is a record
    public record PaymentTimeoutReached : Base.Message
    {
        public Guid OrderId { get; init; }
        public Guid CartId { get; init; }
        public int PaymentTimeoutMinutes { get; init; } = 5;
    }
}