using Common.Contracts.Commands;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentTypeMicroservice.Models;
using PaymentTypeMicroservice.Data;

namespace PaymentTypeMicroservice.Consumers
{
    public class ProcessPaymentRequestedConsumer : IConsumer<ProcessPaymentRequested>
    {
        private readonly PaymentDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProcessPaymentRequestedConsumer> _logger;

        public ProcessPaymentRequestedConsumer(
            PaymentDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            ILogger<ProcessPaymentRequestedConsumer> logger)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessPaymentRequested> context)
        {
            _logger.LogInformation("ProcessPaymentRequested received. OrderId: {OrderId}, Amount: {Amount}, CorrelationId: {CorrelationId}, PaymentTypeId: {PaymentTypeId}",
                context.Message.OrderId, context.Message.Amount, context.Message.CorrelationId, context.Message.PaymentTypeId);

            try
            {
                // Simple direct validation - PaymentTypeId must be between 1 and 3
                bool isPaymentTypeValid = context.Message.PaymentTypeId >= 1 && context.Message.PaymentTypeId <= 3;
                
                if (isPaymentTypeValid)
                {
                    _logger.LogInformation("Payment type {PaymentTypeId} is valid", context.Message.PaymentTypeId);

                    // Create a simple payment record for reference
                    var payment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        CartId = context.Message.CartId,
                        OrderId = context.Message.OrderId,
                        Amount = context.Message.Amount,
                        PaymentTypeId = context.Message.PaymentTypeId,
                        Status = "Approved",
                        TransactionId = Guid.NewGuid().ToString("N")[..16],
                        CreatedAt = DateTime.UtcNow,
                        ProcessedAt = DateTime.UtcNow
                    };
                    
                    // Still save to database for audit/tracking
                    _dbContext.Payments.Add(payment);
                    await _dbContext.SaveChangesAsync();

                    await _publishEndpoint.Publish(new PaymentProcessed
                    {
                        PaymentId = payment.Id,
                        CartId = context.Message.CartId,
                        OrderId = context.Message.OrderId,
                        Amount = context.Message.Amount,
                        TransactionId = payment.TransactionId,
                        CorrelationId = context.Message.CorrelationId
                    });
                }
                else
                {
                    _logger.LogWarning("Invalid payment type: {PaymentTypeId}", context.Message.PaymentTypeId);
                    
                    // Create a record for failed payments too
                    var payment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        CartId = context.Message.CartId,
                        OrderId = context.Message.OrderId,
                        Amount = context.Message.Amount,
                        PaymentTypeId = context.Message.PaymentTypeId,
                        Status = "Rejected",
                        CreatedAt = DateTime.UtcNow,
                    };
                    
                    _dbContext.Payments.Add(payment);
                    await _dbContext.SaveChangesAsync();

                    await _publishEndpoint.Publish(new PaymentFailed
                    {
                        CartId = context.Message.CartId,
                        OrderId = context.Message.OrderId,
                        Reason = $"Invalid payment type ID: {context.Message.PaymentTypeId}",
                        CorrelationId = context.Message.CorrelationId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment type for OrderId: {OrderId}", context.Message.OrderId);

                await _publishEndpoint.Publish(new PaymentFailed
                {
                    CartId = context.Message.CartId,
                    OrderId = context.Message.OrderId,
                    Reason = ex.Message,
                    CorrelationId = context.Message.CorrelationId
                });
            }
        }
    }
}