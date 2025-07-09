using System.Net.Http.Json;
using ShoppingCartMicroservice_Service.DTOs;

public class ProductItemClient
{
    private readonly HttpClient _http;
    public ProductItemClient(HttpClient http) => _http = http;

    public async Task<ProductItemDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"/api/ProductItems/{id}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ProductItemDto>()
            : null;
    }

    public async Task<List<ProductItemDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var arr = ids?.Distinct().ToArray() ?? Array.Empty<int>();
        if (arr.Length == 0) return new();
        var url = $"/api/ProductItems?ids={string.Join(',', arr)}";
        var response = await _http.GetAsync(url);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<ProductItemDto>>() ?? new()
            : new();
    }
}
