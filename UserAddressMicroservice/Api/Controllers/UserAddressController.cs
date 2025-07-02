using Microsoft.AspNetCore.Mvc;
using UserAddressMicroservice.Data.Dtos;
using UserAddressMicroservice.Service.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;

namespace UserAddressMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAddressController : ControllerBase
{
    private readonly IUserAddressService _service;
    private readonly IHttpClientFactory _httpClientFactory;

    public UserAddressController(IUserAddressService service, IHttpClientFactory httpClientFactory)
    {
        _service = service;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserAddressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Kullanıcı kontrolü
        var userClient = _httpClientFactory.CreateClient("UserService");
        var userResponse = await userClient.GetAsync($"api/user/{dto.UserId}");
        if (!userResponse.IsSuccessStatusCode)
            return BadRequest($"Kullanıcı {dto.UserId} bulunamadı (User mikroservis).");

        // Adres kontrolü
        var addressClient = _httpClientFactory.CreateClient("AddressService");
        var addressResponse = await addressClient.GetAsync($"api/address/{dto.AddressId}");
        if (!addressResponse.IsSuccessStatusCode)
            return BadRequest($"Adres {dto.AddressId} bulunamadı (Address mikroservis).");

        return Ok(await _service.CreateAsync(dto));
    }

    [HttpDelete("{userId}/{addressId}")]
    public async Task<IActionResult> Delete(int userId, int addressId)
        => await _service.DeleteAsync(userId, addressId) ? NoContent() : NotFound();

    [HttpGet("{userId}/with-addresses")]
    public async Task<IActionResult> GetUserWithAddresses(int userId)
    {
        var all = await _service.GetAllAsync();
        var userAddresses = all.Where(x => x.UserId == userId).ToList();

        if (!userAddresses.Any()) return NotFound("Kullanıcının adresi bulunamadı.");

        var client = _httpClientFactory.CreateClient("AddressService");

        var addressTasks = userAddresses.Select(x =>
            client.GetFromJsonAsync<object>($"api/address/{x.AddressId}")
        );

        var addresses = await Task.WhenAll(addressTasks);

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
        // Kullanıcı kontrolü (User mikroservisinden)
        var userClient = _httpClientFactory.CreateClient("UserService");
        var userResponse = await userClient.GetAsync($"api/user/{userId}");
        if (!userResponse.IsSuccessStatusCode)
            return NotFound($"Kullanıcı {userId} bulunamadı.");

        // Kullanıcının bu adrese sahip olup olmadığını kontrol et
        var link = await _service.GetAsync(userId, addressId);
        if (link == null)
            return BadRequest($"Kullanıcının {addressId} ID'li adresi bulunmamaktadır.");

        // Adres mikroservisine PUT isteği gönder
        var addressClient = _httpClientFactory.CreateClient("AddressService");
        var response = await addressClient.PutAsJsonAsync($"api/address/{addressId}", updatedUserAddressDto);

        if (!response.IsSuccessStatusCode)
            return BadRequest("Adres güncellenemedi. Adres mikroservisinden hata döndü.");

        return NoContent();
    }

}
