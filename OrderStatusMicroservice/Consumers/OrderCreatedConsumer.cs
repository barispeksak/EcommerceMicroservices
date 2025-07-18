using System;
using System.Threading.Tasks;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderStatusMicroservice.Models;
using OrderStatusMicroservice.Data.Repositories;

namespace OrderStatusMicroservice.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly IOrderStatusRepository _repository;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(
            IOrderStatusRepository repository,
            ILogger<OrderCreatedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order created event received: OrderId={OrderId}, CorrelationId={CorrelationId}", 
                message.OrderId, message.CorrelationId);

            var orderStatus = new OrderStatus
            {
                OrderId = message.OrderId,
                // Map ShopOrderId from OrderId if needed
                ShopOrderId = 0, // You'll need to determine how to map this
                Status = "Confirmed",
                City = "Processing Center", // Default value
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(orderStatus);
            
            _logger.LogInformation("Order status created: OrderId={OrderId}, Status={Status}", 
                orderStatus.OrderId, orderStatus.Status);
        }
    }
}