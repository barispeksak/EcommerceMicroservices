using Microsoft.AspNetCore.Mvc;
using OrderSummaryMicroservice.DTOs;
using OrderSummaryMicroservice.Services;

namespace OrderSummaryMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderSummaryController : ControllerBase
    {
        private readonly IOrderSummaryService _orderSummaryService;

        public OrderSummaryController(IOrderSummaryService orderSummaryService)
        {
            _orderSummaryService = orderSummaryService;
        }

        // GET: api/ordersummary
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAll()
        {
            var orderSummaries = await _orderSummaryService.GetAllAsync();
            return Ok(orderSummaries);
        }

        // GET: api/ordersummary/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderSummaryDto>> Get(int id)
        {
            var orderSummary = await _orderSummaryService.GetByIdAsync(id);
            if (orderSummary == null)
            {
                return NotFound($"OrderSummary with ID {id} not found");
            }
            return Ok(orderSummary);
        }

        // POST: api/ordersummary
        [HttpPost]
        public async Task<ActionResult<OrderSummaryDto>> Post([FromBody] CreateOrderSummaryDto createOrderSummaryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderSummary = await _orderSummaryService.CreateAsync(createOrderSummaryDto);
            return CreatedAtAction(nameof(Get), new { id = orderSummary.Id }, orderSummary);
        }

        // PUT: api/ordersummary/5
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderSummaryDto>> Put(int id, [FromBody] UpdateOrderSummaryDto updateOrderSummaryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderSummary = await _orderSummaryService.UpdateAsync(id, updateOrderSummaryDto);
            if (orderSummary == null)
            {
                return NotFound($"OrderSummary with ID {id} not found");
            }

            return Ok(orderSummary);
        }

        // DELETE: api/ordersummary/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _orderSummaryService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound($"OrderSummary with ID {id} not found");
            }

            return NoContent();
        }
    }
}