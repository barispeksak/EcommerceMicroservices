using System.Net.Http.Json;
using ShoppingCartMicroservice_Service.DTOs;

public class ProductClient
{
    private readonly HttpClient _client;
    public ProductClient(HttpClient client) => _client = client;

    // ProductItem - TEK
    public async Task<ProductItemDto?> GetProductItemByIdAsync(int productItemId)
    {
        var r = await _client.GetAsync($"http://localhost:5127/api/ProductItems/{productItemId}");
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<ProductItemDto>()
            : null;
    }

    // ProductItem - ÇOKLU
    public async Task<List<ProductItemDto>> GetProductItemsByIdsAsync(IEnumerable<int> ids)
    {
        var arr = ids?.Distinct().ToArray() ?? Array.Empty<int>();
        if (arr.Length == 0) return new();

        var url = $"http://localhost:5127/api/ProductItems?ids={string.Join(',', arr)}";
        var r = await _client.GetAsync(url);
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<List<ProductItemDto>>() ?? new()
            : new();
    }

    // Product - TEK
    public async Task<ProductDto?> GetProductByIdAsync(int productId)
    {
        var r = await _client.GetAsync($"http://localhost:5190/api/Products/{productId}");
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<ProductDto>()
            : null;
    }

    // Product - ÇOKLU
    public async Task<List<ProductDto>> GetProductsByIdsAsync(IEnumerable<int> ids)
    {
        var arr = ids?.Distinct().ToArray() ?? Array.Empty<int>();
        if (arr.Length == 0) return new();

        var url = $"http://localhost:5190/api/Products?ids={string.Join(',', arr)}";
        var r = await _client.GetAsync(url);
        return r.IsSuccessStatusCode
            ? await r.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new()
            : new();
    }
}
