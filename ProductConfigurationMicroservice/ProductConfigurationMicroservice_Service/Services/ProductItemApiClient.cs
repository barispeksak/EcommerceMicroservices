using System.Net.Http.Json;

namespace ProductConfigurationMicroservice_Service.Services;

public class ProductItemApiClient
{
    private readonly HttpClient _client;
    public ProductItemApiClient(HttpClient client) => _client = client;

    public async Task<(bool ok, string sku)> TryGetAsync(int productItemId)
    {
        var resp = await _client.GetAsync($"http://productitemmicroservice:8080/api/ProductItems/{productItemId}");
        if (!resp.IsSuccessStatusCode) return (false, "");
        var dto = await resp.Content.ReadFromJsonAsync<ItemLite>();
        return (true, dto?.Sku ?? "");
    }

    private sealed record ItemLite(int Id, string Sku);
}
