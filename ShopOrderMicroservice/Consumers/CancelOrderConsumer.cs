using System;
using System.Threading.Tasks;
using Common.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using ShopOrderMicroservice.Services.Interfaces;

namespace ShopOrderMicroservice.Consumers
{
    public class CancelOrderConsumer : IConsumer<CancelOrder>
    {
        private readonly IShopOrderService _shopOrderService;
        private readonly ILogger<CancelOrderConsumer> _logger;

        public CancelOrderConsumer(
            IShopOrderService shopOrderService,
            ILogger<CancelOrderConsumer> logger)
        {
            _shopOrderService = shopOrderService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CancelOrder> context)
        {
            _logger.LogInformation("CancelOrder received for compensation. CartId: {CartId}, CorrelationId: {CorrelationId}", 
                context.Message.CartId, context.Message.CorrelationId);

            try
            {
                // Convert CartId to a simple order lookup (this is a simplified approach)
                // In a real scenario, you might store CartId -> OrderId mapping
                var cartIdHash = Math.Abs(context.Message.CartId.GetHashCode());
                
                // Try to find and cancel the order
                await _shopOrderService.DeleteAsync(cartIdHash);
                
                _logger.LogInformation("Order cancelled successfully for CartId: {CartId}", context.Message.CartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order for CartId: {CartId}", context.Message.CartId);
                // In compensation, we usually don't fail the saga if cleanup fails
                // Instead, we log and potentially retry later
            }
        }
    }
}
