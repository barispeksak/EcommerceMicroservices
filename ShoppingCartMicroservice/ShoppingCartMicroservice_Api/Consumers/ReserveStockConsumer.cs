using Common.Contracts.Commands;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using ShoppingCartMicroservice_Api.Storage;

namespace ShoppingCartMicroservice_Api.Consumers
{
    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        private readonly ILogger<ReserveStockConsumer> _logger;
        private readonly IStockRepository _stock;

        public ReserveStockConsumer(
            ILogger<ReserveStockConsumer> logger,
            IStockRepository stock)
        {
            _logger = logger;
            _stock = stock;
        }

        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            var cartId = context.Message.CartId;
            var items = await _stock.GetCartItemsAsync(cartId);
            var ok = await _stock.TryReserveAsync(items);

            if (ok)
            {
                await context.Publish(new StockReserved
                {
                    CartId = cartId,
                    CorrelationId = context.Message.CorrelationId,
                    SourceService = "ShoppingCart"
                });
            }
            else
            {
                await context.Publish(new StockNotAvailable
                {
                    CartId = cartId,
                    CorrelationId = context.Message.CorrelationId,
                    SourceService = "ShoppingCart"
                });
            }
        }
    }
}