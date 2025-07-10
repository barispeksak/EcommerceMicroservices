using Microsoft.AspNetCore.Mvc;
using UserAddressMicroservice.Data.Dtos;
using UserAddressMicroservice.Service.Interfaces;
using UserAddressMicroservice.Service.Logging;
using UserAddressMicroservice.Models;
using MongoDB.Bson;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;

namespace UserAddressMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAddressController : ControllerBase
{
    private readonly IUserAddressService _service;
    private readonly IHttpClientFactory  _http;
    private readonly UserAddressActionLogger _logger;

    public UserAddressController(
        IUserAddressService service,
        IHttpClientFactory  httpClientFactory,
        UserAddressActionLogger logger)
    {
        _service = service;
        _http    = httpClientFactory;
        _logger  = logger;
    }

    /* ─────────────── Yardımcılar ─────────────── */

    private static BsonDocument Wrap(object? o) =>
        o is null
            ? new BsonDocument { { "msg", "null" } }
            : BsonDocument.Parse(JsonSerializer.Serialize(o));

    private string CorrelationId() =>
        HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? HttpContext.TraceIdentifier;

    private string PerformedBy() =>
        HttpContext.Request.Headers["X-User-Email"].FirstOrDefault()
        ?? User?.FindFirst(ClaimTypes.Email)?.Value
        ?? "anonymous";

    /* ─────────────── ENDPOINTS ─────────────── */

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId    = CorrelationId(),
            Action           = "GetAll",
            Status           = "Success",
            Message          = "Tüm kullanıcı-adres bağlantıları getirildi.",
            PerformedByEmail = PerformedBy(),
            Description      = Wrap(new { Count = result.Count() })
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserAddressDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userClient    = _http.CreateClient("UserService");
        var addressClient = _http.CreateClient("AddressService");

        var userOk    = (await userClient.GetAsync($"api/user/{dto.UserId}")).IsSuccessStatusCode;
        var addressOk = (await addressClient.GetAsync($"api/address/{dto.AddressId}")).IsSuccessStatusCode;

        if (!userOk || !addressOk)
        {
            var failMsg = !userOk
                ? $"Kullanıcı {dto.UserId} bulunamadı."
                : $"Adres {dto.AddressId} bulunamadı.";

            await _logger.LogAsync(new UserAddressActionLog
            {
                CorrelationId    = CorrelationId(),
                Action           = "Create",
                Status           = "Fail",
                Message          = failMsg,
                PerformedByEmail = PerformedBy(),
                Description      = Wrap(dto)
            });

            return BadRequest(failMsg);
        }

        var created = await _service.CreateAsync(dto);

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId    = CorrelationId(),
            Action           = "Create",
            Status           = "Success",
            Message          = "User-Address bağlantısı oluşturuldu.",
            PerformedByEmail = PerformedBy(),
            Description      = Wrap(dto)
        });

        return Ok(created);
    }

    [HttpDelete("{userId:int}/{addressId:int}")]
    public async Task<IActionResult> Delete(int userId, int addressId)
    {
        var success = await _service.DeleteAsync(userId, addressId);

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId    = CorrelationId(),
            Action           = "Delete",
            Status           = success ? "Success" : "Fail",
            Message          = success
                               ? "User-Address bağlantısı silindi."
                               : "User-Address bağlantısı bulunamadı.",
            PerformedByEmail = PerformedBy(),
            Description      = Wrap(new { UserId = userId, AddressId = addressId })
        });

        return success ? NoContent() : NotFound();
    }

    [HttpGet("{userId:int}/with-addresses")]
    public async Task<IActionResult> GetUserWithAddresses(int userId)
    {
        var links = (await _service.GetAllAsync())
                    .Where(x => x.UserId == userId)
                    .ToList();

        if (!links.Any())
        {
            await _logger.LogAsync(new UserAddressActionLog
            {
                CorrelationId    = CorrelationId(),
                Action           = "GetUserWithAddresses",
                Status           = "Fail",
                Message          = "Kullanıcının adresi bulunamadı.",
                PerformedByEmail = PerformedBy(),
                Description      = Wrap(new { UserId = userId })
            });
            return NotFound("Kullanıcının adresi bulunamadı.");
        }

        var addressClient = _http.CreateClient("AddressService");
        var tasks = links.Select(l => addressClient.GetFromJsonAsync<object>($"api/address/{l.AddressId}"));
        var addresses = await Task.WhenAll(tasks);

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId    = CorrelationId(),
            Action           = "GetUserWithAddresses",
            Status           = "Success",
            Message          = "Kullanıcının adresleri getirildi.",
            PerformedByEmail = PerformedBy(),
            Description      = Wrap(new { UserId = userId, AddressCount = addresses.Length })
        });

        return Ok(new
        {
            UserId       = userId,
            AddressCount = addresses.Length,
            Addresses    = addresses
        });
    }

    [HttpPut("{userId:int}/update-address/{addressId:int}")]
    public async Task<IActionResult> UpdateUserAddress(
        int userId, int addressId, UpdateUserAddressDto dto)
    {
        var userClient    = _http.CreateClient("UserService");
        var addressClient = _http.CreateClient("AddressService");

        if (!(await userClient.GetAsync($"api/user/{userId}")).IsSuccessStatusCode)
            return NotFound($"Kullanıcı {userId} bulunamadı.");

        if (await _service.GetAsync(userId, addressId) == null)
            return BadRequest($"Kullanıcının {addressId} ID'li adresi yok.");

        var response = await addressClient.PutAsJsonAsync($"api/address/{addressId}", dto);
        var success  = response.IsSuccessStatusCode;

        await _logger.LogAsync(new UserAddressActionLog
        {
            CorrelationId    = CorrelationId(),
            Action           = "UpdateUserAddress",
            Status           = success ? "Success" : "Fail",
            Message          = success ? "Adres güncellendi." : "Adres güncellenemedi.",
            PerformedByEmail = PerformedBy(),
            Description      = Wrap(new { UserId = userId, AddressId = addressId })
        });

        return success ? NoContent() : BadRequest("Adres güncellenemedi.");
    }
}
