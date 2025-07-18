using System;
using System.Threading.Tasks;
using Common.Contracts.Commands;
using Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using ShopOrderMicroservice.Services.Interfaces;
using ShopOrderMicroservice.Data.Dtos;
using System.Text.Json;

namespace ShopOrderMicroservice.Consumers
{
    public class CreateOrderRequestedConsumer : IConsumer<CreateOrderRequested>
    {
        private readonly IShopOrderService _shopOrderService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CreateOrderRequestedConsumer> _logger;

        public CreateOrderRequestedConsumer(
            IShopOrderService shopOrderService,
            IPublishEndpoint publishEndpoint,
            ILogger<CreateOrderRequestedConsumer> logger)
        {
            _shopOrderService = shopOrderService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateOrderRequested> context)
        {
            _logger.LogError("🚨 DEBUG: Consumer ENTRY POINT - CartId: {CartId}", context.Message.CartId);
            
            _logger.LogInformation("CreateOrderRequested received. CartId: {CartId}, CorrelationId: {CorrelationId}", 
                context.Message.CartId, context.Message.CorrelationId);

            try
            {
                // Create order DTO with minimal required data for saga testing
                var createOrderDto = new CreateShopOrderDto
                {
                    UserId = 1, // Default test user
                    PaymentTypeId = 1, // Default payment type
                    ShippingAddressId = 1, // Default address
                    ShippingTypeId = 1, // Default shipping
                    ShopId = Math.Abs(context.Message.CartId.GetHashCode()) // Convert Guid to positive int
                };

                // Create the order
                var order = await _shopOrderService.CreateAsync(createOrderDto);
                
                if (order == null)
                {
                    var errorMsg = "Failed to create order in database";
                    _logger.LogError(errorMsg);
                    
                    await _publishEndpoint.Publish(new OrderFailed
                    {
                        CartId = context.Message.CartId,
                        CorrelationId = context.Message.CorrelationId,
                        Reason = errorMsg
                    });
                    return;
                }

                // SUCCESS: Convert int OrderId to Guid for saga compatibility
                // Create a deterministic GUID from the integer OrderId
                var orderIdBytes = BitConverter.GetBytes(order.Id);
                var guidBytes = new byte[16];
                
                // Copy the OrderId bytes to the first part of the GUID
                Array.Copy(orderIdBytes, guidBytes, orderIdBytes.Length);
                
                // Fill the rest with a fixed pattern or zeros
                var orderGuid = new Guid(guidBytes);

                _logger.LogInformation("Converted OrderId {OrderId} to GUID {OrderGuid}",
                    order.Id, orderGuid);
                
                _logger.LogInformation("Order created successfully. OrderId: {OrderId} (converted to {OrderGuid}), CartId: {CartId}", 
                    order.Id, orderGuid, context.Message.CartId);

                await _publishEndpoint.Publish(new OrderCreated
                {
                    OrderId = orderGuid, // Convert int to Guid
                    CartId = context.Message.CartId,
                    CorrelationId = context.Message.CorrelationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for CartId: {CartId}", context.Message.CartId);
                
                await _publishEndpoint.Publish(new OrderFailed
                {
                    CartId = context.Message.CartId,
                    CorrelationId = context.Message.CorrelationId,
                    Reason = ex.Message
                });
            }
        }
    }
}