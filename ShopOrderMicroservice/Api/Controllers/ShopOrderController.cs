using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ShopOrderMicroservice.Data.Dtos;
using ShopOrderMicroservice.Services.Interfaces;
using ShopOrderMicroservice.Services.Logging;
using ShopOrderMicroservice.Models;
using System.Net.Http;
using System.Text.Json; // <-- Helper fonksiyonu için

namespace ShopOrderMicroservice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopOrderController : ControllerBase
    {
        private readonly IShopOrderService _service;
        private readonly ShopOrderActionLogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopOrderController(
            IShopOrderService service,
            ShopOrderActionLogger logger,
            IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // --- HELPER FUNCTIONS ---
        private static BsonDocument WrapObject(object? obj) =>
            obj is null ? new BsonDocument { { "msg", "null" } } : BsonDocument.Parse(JsonSerializer.Serialize(obj));

        private string GetCorrelationId() =>
            HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

        private string GetPerformedByEmail() =>
            HttpContext.Request.Headers["X-User-Email"].FirstOrDefault() ?? "anonymous";

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateShopOrderDto dto)
        {
            var cid = GetCorrelationId();
            var email = GetPerformedByEmail();

            // Kullanıcı kontrolü
            var userClient = _httpClientFactory.CreateClient("UserService");
            if (!(await userClient.GetAsync($"api/user/{dto.UserId}")).IsSuccessStatusCode)
            {
                var msg = $"Sipariş oluşturulamadı: Kullanıcı bulunamadı (UserId={dto.UserId})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Create",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            // Adres kontrolü
            var addressClient = _httpClientFactory.CreateClient("AddressService");
            if (!(await addressClient.GetAsync($"api/address/{dto.ShippingAddressId}")).IsSuccessStatusCode)
            {
                var msg = $"Sipariş oluşturulamadı: Teslimat adresi bulunamadı (ShippingAddressId={dto.ShippingAddressId})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Create",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            // Kargo tipi kontrolü
            var shippingClient = _httpClientFactory.CreateClient("ShippingService");
            if (!(await shippingClient.GetAsync($"api/shippingtype/{dto.ShippingTypeId}")).IsSuccessStatusCode)
            {
                var msg = $"Sipariş oluşturulamadı: Kargo tipi bulunamadı (ShippingTypeId={dto.ShippingTypeId})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Create",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            // Ödeme tipi kontrolü
            var paymentClient = _httpClientFactory.CreateClient("PaymentService");
            if (!(await paymentClient.GetAsync($"api/paymenttype/{dto.PaymentTypeId}")).IsSuccessStatusCode)
            {
                var msg = $"Sipariş oluşturulamadı: Ödeme tipi bulunamadı (PaymentTypeId={dto.PaymentTypeId})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Create",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            // Sepet kontrolü
            var cartClient = _httpClientFactory.CreateClient("ShoppingCartService");
            if (!(await cartClient.GetAsync($"api/shoppingcart/{dto.ShopId}")).IsSuccessStatusCode)
            {
                var msg = $"Sipariş oluşturulamadı: Kullanıcıya ait alışveriş sepeti bulunamadı (ShopId={dto.ShopId})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Create",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            // Siparişi oluştur
            var result = await _service.CreateAsync(dto);
            if (result == null)
            {
                var msg = "Sipariş oluşturulamadı: Sistem hatası veya eksik veri.";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Create",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = WrapObject(dto)
                });
                return StatusCode(500, new { message = msg });
            }

            // Başarılı: Sipariş oluşturuldu
            await _logger.LogAsync(new ShopOrderActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = email,
                Action = "Create",
                Status = "Success",
                Message = $"Sipariş başarıyla oluşturuldu (OrderId={result.Id})",
                Timestamp = DateTime.UtcNow,
                Description = WrapObject(result)
            });

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cid = GetCorrelationId();
            var email = GetPerformedByEmail();

            var result = await _service.GetAllAsync();

            await _logger.LogAsync(new ShopOrderActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = email,
                Action = "GetAll",
                Status = "Success",
                Message = "Tüm siparişler listelendi.",
                Timestamp = DateTime.UtcNow,
                Description = WrapObject(result)
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cid = GetCorrelationId();
            var email = GetPerformedByEmail();

            var result = await _service.GetByIdAsync(id);
            if (result == null)
            {
                var msg = $"Sipariş bulunamadı (OrderId={id})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "GetById",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = new BsonDocument { { "OrderId", id } }
                });
                return NotFound(new { message = msg });
            }

            await _logger.LogAsync(new ShopOrderActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = email,
                Action = "GetById",
                Status = "Success",
                Message = "Sipariş detayları getirildi.",
                Timestamp = DateTime.UtcNow,
                Description = WrapObject(result)
            });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShopOrderDto dto)
        {
            var cid = GetCorrelationId();
            var email = GetPerformedByEmail();

            var success = await _service.UpdateAsync(id, dto);
            if (!success)
            {
                var msg = $"Sipariş güncellenemedi (OrderId={id})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Update",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = WrapObject(dto)
                });
                return BadRequest(new { message = msg });
            }

            await _logger.LogAsync(new ShopOrderActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = email,
                Action = "Update",
                Status = "Success",
                Message = $"Sipariş başarıyla güncellendi (OrderId={id})",
                Timestamp = DateTime.UtcNow,
                Description = WrapObject(dto)
            });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cid = GetCorrelationId();
            var email = GetPerformedByEmail();

            var success = await _service.DeleteAsync(id);
            if (!success)
            {
                var msg = $"Sipariş silinemedi: Sipariş bulunamadı (OrderId={id})";
                await _logger.LogAsync(new ShopOrderActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = email,
                    Action = "Delete",
                    Status = "Fail",
                    Message = msg,
                    Timestamp = DateTime.UtcNow,
                    Description = new BsonDocument { { "OrderId", id } }
                });
                return NotFound(new { message = msg });
            }

            await _logger.LogAsync(new ShopOrderActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = email,
                Action = "Delete",
                Status = "Success",
                Message = $"Sipariş başarıyla silindi (OrderId={id})",
                Timestamp = DateTime.UtcNow,
                Description = new BsonDocument { { "OrderId", id } }
            });

            return NoContent();
        }
    }
}
