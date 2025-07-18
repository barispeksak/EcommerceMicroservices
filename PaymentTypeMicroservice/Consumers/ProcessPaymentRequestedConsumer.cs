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

        // ... mevcut kod ...
        public async Task Consume(ConsumeContext<ProcessPaymentRequested> context)
        {
            _logger.LogInformation("ProcessPaymentRequested received. OrderId: {OrderId}, Amount: {Amount}, CorrelationId: {CorrelationId}",
                context.Message.OrderId, context.Message.Amount, context.Message.CorrelationId);

            try
            {
                // 1. Payment nesnesini oluştur
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    CartId = context.Message.CartId,
                    OrderId = context.Message.OrderId,
                    Amount = context.Message.Amount,
                    PaymentTypeId = context.Message.PaymentTypeId,
                    Status = "Processing",
                    CorrelationId = context.Message.CorrelationId,
                    CreatedAt = DateTime.UtcNow
                };

                // 2. Veritabanına kaydet
                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();

                // 3. Gerçek ödeme işlemini simüle et
                var isPaymentSuccessful = await ProcessPaymentAsync(payment);

                if (isPaymentSuccessful)
                {
                    // 4. Başarılıysa güncelle
                    payment.Status = "Completed";
                    payment.TransactionId = Guid.NewGuid().ToString("N")[..16];
                    payment.ProcessedAt = DateTime.UtcNow;
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
                    // 5. Başarısızsa güncelle
                    payment.Status = "Failed";
                    await _dbContext.SaveChangesAsync();

                    await _publishEndpoint.Publish(new PaymentFailed
                    {
                        CartId = context.Message.CartId,
                        OrderId = context.Message.OrderId,
                        Reason = "Payment gateway declined the transaction",
                        CorrelationId = context.Message.CorrelationId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for OrderId: {OrderId}", context.Message.OrderId);

                await _publishEndpoint.Publish(new PaymentFailed
                {
                    CartId = context.Message.CartId,
                    OrderId = context.Message.OrderId,
                    Reason = ex.Message,
                    CorrelationId = context.Message.CorrelationId
                });
            }
        }

        private async Task<bool> ProcessPaymentAsync(Payment payment)
        {
            // Simulate payment gateway processing
            await Task.Delay(100); // Simulate network call

            // Simple mock logic: fail payments over $1000, succeed others
            return payment.Amount <= 1000m;
        }
    }
}