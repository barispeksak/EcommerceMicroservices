using Common.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using ShoppingCartMicroservice_Api.Storage;

namespace ShoppingCartMicroservice_Api.Consumers;

public class RollbackStockConsumer : IConsumer<RollbackStock>
{
    private readonly ILogger<RollbackStockConsumer> _logger;
    private readonly IStockRepository _stock;

    public RollbackStockConsumer(
        ILogger<RollbackStockConsumer> logger,
        IStockRepository stock)
    {
        _logger = logger;
        _stock  = stock;
    }

    public async Task Consume(ConsumeContext<RollbackStock> ctx)
    {
        await _stock.ReleaseReservationAsync(ctx.Message.CartId);
        _logger.LogInformation("RollbackStock işlendi. CartId: {CartId}", ctx.Message.CartId);
    }
}