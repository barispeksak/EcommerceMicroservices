using System.Net.Http.Json;
using ShoppingCartMicroservice_Service.DTOs;

public class ProductClient
{
    private readonly HttpClient _http;
    public ProductClient(HttpClient http) => _http = http;

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"/api/Products/{id}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ProductDto>()
            : null;
    }

    public async Task<List<ProductDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var arr = ids?.Distinct().ToArray() ?? Array.Empty<int>();
        if (arr.Length == 0) return new();
        var url = $"/api/Products?ids={string.Join(',', arr)}";
        var response = await _http.GetAsync(url);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new()
            : new();
    }
}
