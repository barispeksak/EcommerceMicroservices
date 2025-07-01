using System.Net.Http.Json;

namespace ProductConfigurationMicroservice_Service.Services;

public class VariationOptionApiClient
{
    private readonly HttpClient _client;
    public VariationOptionApiClient(HttpClient client) => _client = client;

    public async Task<(bool ok, string value)> TryGetAsync(int optionId)
    {
        var resp = await _client.GetAsync($"http://localhost:5002/api/VariationOptions/{optionId}");
        if (!resp.IsSuccessStatusCode) return (false, "");
        var dto = await resp.Content.ReadFromJsonAsync<OptionLite>();
        return (true, dto?.Value ?? "");
    }

    private sealed record OptionLite(int Id, string Value);
}