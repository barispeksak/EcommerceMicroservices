using System;
using System.Threading.Tasks;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderStatusMicroservice.Models;
using OrderStatusMicroservice.Data.Repositories;


namespace OrderStatusMicroservice.Consumers
{
    public class StockNotAvailableConsumer : IConsumer<StockNotAvailable>
    {
        private readonly IOrderStatusRepository _repository;
        private readonly ILogger<StockNotAvailableConsumer> _logger;

        public StockNotAvailableConsumer(
            IOrderStatusRepository repository,
            ILogger<StockNotAvailableConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockNotAvailable> context)
        {
            var message = context.Message;
            _logger.LogInformation("Stock not available event received: CartId={CartId}, CorrelationId={CorrelationId}", 
                message.CartId, message.CorrelationId);

            var orderStatus = new OrderStatus
            {
                // No OrderId yet since order wasn't created
                OrderId = Guid.Empty,
                ShopOrderId = 0,
                Status = "Stock Unavailable",
                City = "Warehouse",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ErrorMessage = "Required items are out of stock"
            };

            await _repository.AddAsync(orderStatus);
            
            _logger.LogInformation("Stock unavailable status created, Status={Status}", orderStatus.Status);
        }
    }
}