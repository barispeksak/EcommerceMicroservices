using System.Net.Http.Json;

namespace ShoppingCartMicroservice_Service.Services;

public class ProductItemApiClient
{
    private readonly HttpClient _client;
    public ProductItemApiClient(HttpClient client) => _client = client;

    public async Task<(bool ok, decimal price)> TryGetAsync(int productItemId)
    {
        var resp = await _client.GetAsync($"http://localhost:5127/api/ProductItems/{productItemId}");
        if (!resp.IsSuccessStatusCode) return (false, 0m);

        var dto = await resp.Content.ReadFromJsonAsync<ItemLite>();
        return dto is null ? (false, 0m) : (true, dto.Price);
    }

    private sealed record ItemLite(int Id, string Sku, decimal Price);

}