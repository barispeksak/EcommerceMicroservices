using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Contracts.Commands;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OrderSagaOrchestrator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SagaController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<SagaController> _logger;

        public SagaController(IPublishEndpoint publishEndpoint, ILogger<SagaController> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            // Validate request
            if (request.CartId == Guid.Empty)
            {
                return BadRequest("CartId is required");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest("At least one item is required");
            }

            // Generate a correlation ID for tracking this saga instance
            var correlationId = Guid.NewGuid();
            
            _logger.LogInformation("Starting new order saga with CorrelationId: {CorrelationId}, CartId: {CartId}", 
                correlationId, request.CartId);

            // Create and publish the event that starts the saga
            await _publishEndpoint.Publish(new FinalStockCheckRequested(request.CartId, request.Items)
            {
                CorrelationId = correlationId,
                SourceService = "SagaApi"
            });

            // Return 202 Accepted with the correlation ID for tracking
            return Accepted(new { 
                CorrelationId = correlationId,
                Message = "Order processing started. Use the correlationId to track your order."
            });
        }
    }

    // DTO for the request
    public class PlaceOrderRequest
    {
        public Guid CartId { get; set; }
        public List<Guid> Items { get; set; } = new();
    }
}