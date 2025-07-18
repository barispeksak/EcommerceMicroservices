using Common.Contracts.Commands;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using ShoppingCartMicroservice_Api.Storage;

namespace ShoppingCartMicroservice_Api.Consumers;

public class FinalStockCheckRequestedConsumer : IConsumer<FinalStockCheckRequested>
{
    private readonly ILogger<FinalStockCheckRequestedConsumer> _logger;
    private readonly IStockRepository _stock;

    public FinalStockCheckRequestedConsumer(
        ILogger<FinalStockCheckRequestedConsumer> logger,
        IStockRepository stock)
    {
        _logger = logger;
        _stock  = stock;
    }

    public async Task Consume(ConsumeContext<FinalStockCheckRequested> ctx)
    {
        var cartId = ctx.Message.CartId;
        _logger.LogInformation("FinalStockCheckRequested geldi. CartId: {CartId}", cartId);

        var items = await _stock.GetCartItemsAsync(cartId);
        var ok    = await _stock.TryReserveAsync(items);

        if (ok)
        {
            await ctx.Publish(new StockReserved
            {
                CartId        = cartId,
                CorrelationId = ctx.Message.CorrelationId,
                SourceService = "ShoppingCart"
            });
            _logger.LogInformation("Stok rezerve edildi → StockReserved publish.");
        }
        else
        {
            await ctx.Publish(new StockNotAvailable
            {
                CartId        = cartId,
                CorrelationId = ctx.Message.CorrelationId,
                SourceService = "ShoppingCart"
            });
            _logger.LogWarning("Stok YETERSİZ → StockNotAvailable publish.");
        }
    }
}