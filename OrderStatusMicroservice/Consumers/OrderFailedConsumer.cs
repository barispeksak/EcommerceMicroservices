using System;
using System.Threading.Tasks;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderStatusMicroservice.Models;
using OrderStatusMicroservice.Data.Repositories;


namespace OrderStatusMicroservice.Consumers
{
    public class OrderFailedConsumer : IConsumer<OrderFailed>
    {
        private readonly IOrderStatusRepository _repository;
        private readonly ILogger<OrderFailedConsumer> _logger;

        public OrderFailedConsumer(
            IOrderStatusRepository repository,
            ILogger<OrderFailedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderFailed> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order failed event received: OrderId={OrderId}, Reason={Reason}, CorrelationId={CorrelationId}", 
                message.OrderId, message.Reason, message.CorrelationId);

            var orderStatus = new OrderStatus
            {
                OrderId = message.OrderId,
                // Map ShopOrderId from OrderId if needed
                ShopOrderId = 0, // You'll need to determine how to map this
                Status = "Failed",
                City = "System", // Default for failed orders
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ErrorMessage = message.Reason
            };

            await _repository.AddAsync(orderStatus);
            
            _logger.LogInformation("Failed order status created: OrderId={OrderId}, Status={Status}", 
                orderStatus.OrderId, orderStatus.Status);
        }
    }
}