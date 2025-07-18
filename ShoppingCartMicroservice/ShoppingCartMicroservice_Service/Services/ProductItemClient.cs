using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using ShoppingCartMicroservice_Service.DTOs;

public class ProductItemClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductItemClient> _logger;
    
    public ProductItemClient(HttpClient http, ILogger<ProductItemClient> logger)
    {
        _http = http;
        _logger = logger;
        
        // Set auth token for all requests
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", 
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRlc3QxMjQ1NzhAZXhhbXBsZS5jb20iLCJuYW1laWQiOiIxIiwibmJmIjoxNzUyMDYxMDE1LCJleHAiOjE3NTIwNjE5MTUsImlhdCI6MTc1MjA2MTAxNX0.AtPawofS1gxrrNHLMVkPG39xoftpEveMIfs8R-iHy7o"
        );
    }

    public async Task<ProductItemDto?> GetByIdAsync(int id)
    {
        try {
            _logger.LogInformation("Getting product item by id {Id}", id);
            var response = await _http.GetAsync($"/api/productitems/{id}");
            _logger.LogInformation("GetByIdAsync response status: {Status}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ProductItemDto>();
                _logger.LogInformation("Successfully retrieved product item {Id}", id);
                return result;
            }
            else
            {
                _logger.LogWarning("Failed to get product item {Id}. Status: {Status}", id, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error response content: {Content}", content);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting product item {Id}", id);
            return null;
        }
    }

    public async Task<List<ProductItemDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        try {
            var arr = ids?.Distinct().ToArray() ?? Array.Empty<int>();
            if (arr.Length == 0) return new();
            
            var url = $"/api/productitems?ids={string.Join(',', arr)}";
            _logger.LogInformation("Getting product items with url: {Url}", url);
            
            var response = await _http.GetAsync(url);
            _logger.LogInformation("GetByIdsAsync response status: {Status}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ProductItemDto>>() ?? new();
                _logger.LogInformation("Successfully retrieved {Count} product items", result.Count);
                return result;
            }
            else
            {
                _logger.LogWarning("Failed to get product items. Status: {Status}", response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error response content: {Content}", content);
                return new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting product items by ids");
            return new();
        }
    }
}
