namespace ProductItemMicroservice_Service.Services;

public class ProductApiClient
{
    private readonly HttpClient _client;
    public ProductApiClient(HttpClient client) => _client = client;

    public async Task<bool> ProductExists(int productId)
    {
        // ➡️ Product API’nin dinlediği portu ve route’u kendi ortamına göre güncelle
        var response = await _client.GetAsync($"http://productmicroservice:8080/api/Products/{productId}");
        return response.IsSuccessStatusCode;
    }
}

