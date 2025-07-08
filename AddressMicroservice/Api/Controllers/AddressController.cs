using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using AddressMicroservice.Service.DTOs;
using AddressMicroservice.Service.Interfaces;
using AddressMicroservice.Models;
using AddressMicroservice.Service.Services; // Log servisinin buradan geldiğini varsaydım
using System.Text.Json;

namespace AddressMicroservice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly AddressActionLogger _logger;

        public AddressController(IAddressService addressService, AddressActionLogger logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        // --- Helper'lar ---
        private static BsonDocument WrapString(string msg) =>
            new() { { "msg", msg } };

        private static BsonDocument WrapObject(object? obj) =>
            obj is null
                ? new BsonDocument { { "msg", "null" } }
                : BsonDocument.Parse(JsonSerializer.Serialize(obj));

        private string GetCorrelationId() =>
            HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

        private string GetPerformedByEmail()
        {
            var email = HttpContext.Request.Headers["X-User-Email"].FirstOrDefault();
            return email ?? "anonymous";
        }

        // --- CRUD LOG'LU ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetAll()
        {
            var addresses = await _addressService.GetAllAsync();

            await _logger.LogAsync(new AddressActionLog
            {
                Action = "GetAll",
                Level = "Info",
                Message = "Tüm adresler listelendi.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                Description = WrapString($"Toplam adres sayısı: {addresses.Count()}"),
                PerformedByEmail = GetPerformedByEmail()
            });

            return Ok(addresses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AddressDto>> Get(int id)
        {
            var address = await _addressService.GetByIdAsync(id);

            await _logger.LogAsync(new AddressActionLog
            {
                Action = "GetById",
                Level = address == null ? "Warn" : "Info",
                Message = address == null ? "Adres bulunamadı." : "Adres getirildi.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                AddressId = (address?.Id ?? id).ToString(),
                UserCity = address?.City,
                Phone = address?.Phone,
                Description = address == null
                    ? WrapString($"Id: {id} ile adres bulunamadı.")
                    : WrapObject(address),
                PerformedByEmail = GetPerformedByEmail()
            });

            return address is null ? NotFound($"Address with ID {id} not found") : Ok(address);
        }

        [HttpPost]
        public async Task<ActionResult<AddressDto>> Post([FromBody] CreateAddressDto createAddressDto)
        {
            try
            {
                var address = await _addressService.CreateAsync(createAddressDto);

                await _logger.LogAsync(new AddressActionLog
                {
                    Action = "Create",
                    Level = "Info",
                    Message = $"Adres başarıyla eklendi: {address.Id}",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    AddressId = address.Id.ToString(),
                    UserCity = address.City,
                    Phone = address.Phone,
                    Description = WrapObject(createAddressDto),
                    PerformedByEmail = GetPerformedByEmail()
                });

                return CreatedAtAction(nameof(Get), new { id = address.Id }, address);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(new AddressActionLog
                {
                    Action = "Create",
                    Level = "Error",
                    Message = $"Adres eklenirken hata: {ex.Message}",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    Description = new BsonDocument
                    {
                        { "dto", JsonSerializer.Serialize(createAddressDto) },
                        { "exception", ex.Message }
                    },
                    PerformedByEmail = GetPerformedByEmail()
                });
                return BadRequest($"Adres eklenirken hata oluştu: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AddressDto>> Put(int id, [FromBody] UpdateAddressDto updateAddressDto)
        {
            try
            {
                var address = await _addressService.UpdateAsync(id, updateAddressDto);

                await _logger.LogAsync(new AddressActionLog
                {
                    Action = "Update",
                    Level = address == null ? "Warn" : "Info",
                    Message = address == null
                        ? $"Adres ID {id} güncellenemedi, bulunamadı."
                        : $"Adres {address.Id} başarıyla güncellendi.",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    AddressId = address?.Id.ToString() ?? id.ToString(),
                    UserCity = address?.City,
                    Phone = address?.Phone,
                    Description = address == null
                        ? WrapString("Adres bulunamadı veya güncellenemedi.")
                        : WrapObject(updateAddressDto),
                    PerformedByEmail = GetPerformedByEmail()
                });

                return address == null
                    ? NotFound($"Address with ID {id} not found")
                    : Ok(address);
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(new AddressActionLog
                {
                    Action = "Update",
                    Level = "Error",
                    Message = $"Adres güncellenirken hata: {ex.Message}",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    AddressId = id.ToString(),
                    Description = new BsonDocument
                    {
                        { "dto", JsonSerializer.Serialize(updateAddressDto) },
                        { "exception", ex.Message }
                    },
                    PerformedByEmail = GetPerformedByEmail()
                });
                return BadRequest($"Adres güncellenirken hata oluştu: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var address = await _addressService.GetByIdAsync(id);

            if (address == null)
            {
                await _logger.LogAsync(new AddressActionLog
                {
                    Action = "Delete",
                    Level = "Warn",
                    Message = $"Adres ID {id} silinemedi, bulunamadı.",
                    CorrelationId = GetCorrelationId(),
                    Timestamp = DateTime.UtcNow,
                    AddressId = id.ToString(),
                    Description = WrapString($"Adres {id} bulunamadı."),
                    PerformedByEmail = GetPerformedByEmail()
                });
                return NotFound($"Address with ID {id} not found");
            }

            await _addressService.DeleteAsync(id);

            await _logger.LogAsync(new AddressActionLog
            {
                Action = "Delete",
                Level = "Info",
                Message = $"Adres {address.Id} başarıyla silindi.",
                CorrelationId = GetCorrelationId(),
                Timestamp = DateTime.UtcNow,
                AddressId = address.Id.ToString(),
                UserCity = address.City,
                Phone = address.Phone,
                Description = WrapObject(address),
                PerformedByEmail = GetPerformedByEmail()
            });

            return NoContent();
        }
    }
}

