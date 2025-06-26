using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopOrderController : ControllerBase
    {
        private readonly ShopOrderDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopOrderController(ShopOrderDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // POST: api/shoporder
        [HttpPost]
        public async Task<ActionResult<ShopOrder>> CreateOrder(ShopOrder order)
        {
            // HttpClient'ı oluştur
            var userClient = _httpClientFactory.CreateClient("UserService");
            var addressClient = _httpClientFactory.CreateClient("AddressService");
            var paymentClient = _httpClientFactory.CreateClient("PaymentService");

            // Kullanıcı kontrolü
            var userResponse = await userClient.GetAsync($"api/user/{order.UserId}");
            if (!userResponse.IsSuccessStatusCode)
                return BadRequest($"Kullanıcı {order.UserId} bulunamadı.");

            // Adres kontrolü
            var addressResponse = await addressClient.GetAsync($"api/address/{order.ShippingAddressId}");
            if (!addressResponse.IsSuccessStatusCode)
                return BadRequest($"Adres {order.ShippingAddressId} bulunamadı.");

            // paymentTypr kontolü
            var paymentType = await _context.PaymentTypes.FindAsync(order.PaymentTypeId);
            if (paymentType == null)
                return BadRequest($"PaymentType {order.PaymentTypeId} bulunamadı.");

            // Siparişi veritabanına kaydet
            _context.ShopOrders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // GET: api/shoporder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShopOrder>> GetOrder(int id)
        {
            var order = await _context.ShopOrders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return order;
        }

        // GET: api/shoporder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShopOrder>>> GetAllOrders()
        {
            return await _context.ShopOrders
                                 .ToListAsync();
        }

        // PUT: api/shoporder/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, ShopOrder updatedOrder)
        {
            if (id != updatedOrder.Id)
                return BadRequest("Id alanı uyuşmuyor.");

            // FK kontrolleri (User / Address / Payment mikroservisleri)
            var userClient = _httpClientFactory.CreateClient("UserService");
            var addrClient = _httpClientFactory.CreateClient("AddressService");
            var payClient = _httpClientFactory.CreateClient("PaymentService");

            if (!(await userClient.GetAsync($"api/user/{updatedOrder.UserId}")).IsSuccessStatusCode)
                return BadRequest($"Kullanıcı {updatedOrder.UserId} bulunamadı.");

            if (!(await addrClient.GetAsync($"api/address/{updatedOrder.ShippingAddressId}")).IsSuccessStatusCode)
                return BadRequest($"Adres {updatedOrder.ShippingAddressId} bulunamadı.");

            var paymentType = await _context.PaymentTypes.FindAsync(updatedOrder.PaymentTypeId);
            if (paymentType == null)
                return BadRequest($"PaymentType {updatedOrder.PaymentTypeId} bulunamadı.");

            _context.Entry(updatedOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ShopOrders.Any(o => o.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent(); // 204
        }

        // DELETE: api/shoporder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.ShopOrders
                                      .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            _context.ShopOrders.Remove(order);

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
