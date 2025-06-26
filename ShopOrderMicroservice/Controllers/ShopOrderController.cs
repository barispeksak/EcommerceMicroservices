using Microsoft.AspNetCore.Mvc;
using ShopOrderMicroservice.DTOs;
using ShopOrderMicroservice.Services;

namespace ShopOrderMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopOrderController : ControllerBase
    {
        private readonly IShopOrderService _shopOrderService;

        public ShopOrderController(IShopOrderService shopOrderService)
        {
            _shopOrderService = shopOrderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShopOrderDto>>> GetShopOrders()
        {
            var shopOrders = await _shopOrderService.GetShopOrders();
            return Ok(shopOrders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShopOrderDto>> GetShopOrder(int id)
        {
            var shopOrder = await _shopOrderService.GetShopOrder(id);

            if (shopOrder == null)
            {
                return NotFound();
            }

            return Ok(shopOrder);
        }

        [HttpPost]
        public async Task<ActionResult<ShopOrderDto>> CreateShopOrder(ShopOrderDto shopOrderDto)
        {
            var newShopOrder = await _shopOrderService.CreateShopOrder(shopOrderDto);
            return CreatedAtAction(nameof(GetShopOrder), new { id = newShopOrder.Id }, newShopOrder);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShopOrder(int id, ShopOrderDto shopOrderDto)
        {
            if (id != shopOrderDto.Id)
            {
                return BadRequest();
            }

            var updatedShopOrder = await _shopOrderService.UpdateShopOrder(shopOrderDto);

            if (updatedShopOrder == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShopOrder(int id)
        {
            var result = await _shopOrderService.DeleteShopOrder(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
