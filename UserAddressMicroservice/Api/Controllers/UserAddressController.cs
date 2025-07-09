using Microsoft.AspNetCore.Mvc;
using UserAddressMicroservice.Data.Dtos;
using UserAddressMicroservice.Service.Interfaces;
using UserAddressMicroservice.Service.Logging;
using UserAddressMicroservice.Models;
using MongoDB.Bson;
using System.Net.Http;
using System.Net.Http.Json;

namespace UserAddressMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAddressController : ControllerBase
{
    private readonly IUserAddressService _service;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserAddressActionLogger _logger;

    public UserAddressController(IUserAddressService service, IHttpClientFactory httpClientFactory, UserAddressActionLogger logger)
    {
        _service = service;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId = cid,
            Action = "GetAll",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Tüm kullanıcı-adres bağlantıları getirildi.",
            Description = new BsonDocument { { "Count", result.Count() } }
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserAddressDto dto)
    {
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userClient = _httpClientFactory.CreateClient("UserService");
        var userResponse = await userClient.GetAsync($"api/user/{dto.UserId}");

        if (!userResponse.IsSuccessStatusCode)
        {
            await _logger.LogAsync(new UserAddressActionLog
            {
                CorrelationId = cid,
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = $"Kullanıcı {dto.UserId} bulunamadı.",
            });

            return BadRequest($"Kullanıcı {dto.UserId} bulunamadı (User mikroservis).");
        }

        var addressClient = _httpClientFactory.CreateClient("AddressService");
        var addressResponse = await addressClient.GetAsync($"api/address/{dto.AddressId}");

        if (!addressResponse.IsSuccessStatusCode)
        {
            await _logger.LogAsync(new UserAddressActionLog
            {
                CorrelationId = cid,
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = $"Adres {dto.AddressId} bulunamadı.",
            });

            return BadRequest($"Adres {dto.AddressId} bulunamadı (Address mikroservis).");
        }

        var created = await _service.CreateAsync(dto);

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId = cid,
            Action = "Create",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "User-Address bağlantısı oluşturuldu.",
            Description = new BsonDocument
            {
                { "UserId", dto.UserId },
                { "AddressId", dto.AddressId }
            }
        });

        return Ok(created);
    }

    [HttpDelete("{userId}/{addressId}")]
    public async Task<IActionResult> Delete(int userId, int addressId)
    {
        var success = await _service.DeleteAsync(userId, addressId);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId = cid,
            Action = "Delete",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "User-Address bağlantısı silindi." : "User-Address bağlantısı bulunamadı.",
            Description = new BsonDocument { { "UserId", userId }, { "AddressId", addressId } }
        });

        return success ? NoContent() : NotFound();
    }

    [HttpGet("{userId}/with-addresses")]
    public async Task<IActionResult> GetUserWithAddresses(int userId)
    {
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();
        var all = await _service.GetAllAsync();
        var userAddresses = all.Where(x => x.UserId == userId).ToList();

        if (!userAddresses.Any())
        {
            await _logger.LogAsync(new UserAddressActionLog
            {
                CorrelationId = cid,
                Action = "GetUserWithAddresses",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Kullanıcının adresi bulunamadı.",
                Description = new BsonDocument { { "UserId", userId } }
            });

            return NotFound("Kullanıcının adresi bulunamadı.");
        }

        var client = _httpClientFactory.CreateClient("AddressService");
        var addressTasks = userAddresses.Select(x =>
            client.GetFromJsonAsync<object>($"api/address/{x.AddressId}")
        );

        var addresses = await Task.WhenAll(addressTasks);

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId = cid,
            Action = "GetUserWithAddresses",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Kullanıcının adresleri getirildi.",
            Description = new BsonDocument { { "UserId", userId }, { "AddressCount", addresses.Length } }
        });

        return Ok(new
        {
            UserId = userId,
            AddressCount = addresses.Length,
            Addresses = addresses
        });
    }

    [HttpPut("{userId}/update-address/{addressId}")]
    public async Task<IActionResult> UpdateUserAddress(int userId, int addressId, UpdateUserAddressDto updatedUserAddressDto)
    {
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        var userClient = _httpClientFactory.CreateClient("UserService");
        var userResponse = await userClient.GetAsync($"api/user/{userId}");
        if (!userResponse.IsSuccessStatusCode)
            return NotFound($"Kullanıcı {userId} bulunamadı.");

        var link = await _service.GetAsync(userId, addressId);
        if (link == null)
            return BadRequest($"Kullanıcının {addressId} ID'li adresi bulunmamaktadır.");

        var addressClient = _httpClientFactory.CreateClient("AddressService");
        var response = await addressClient.PutAsJsonAsync($"api/address/{addressId}", updatedUserAddressDto);

        var success = response.IsSuccessStatusCode;

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId = cid,
            Action = "UpdateUserAddress",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "Adres güncellendi." : "Adres güncellenemedi.",
            Description = new BsonDocument
            {
                { "UserId", userId },
                { "AddressId", addressId }
            }
        });

        return success ? NoContent() : BadRequest("Adres güncellenemedi.");
    }
}
