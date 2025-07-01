namespace Variation_OptionMicroservice.Service.Services;

public class CategoryApiClient
{
    private readonly HttpClient _client;
    public CategoryApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> VariationExists(int variationId)
    {
        var response = await _client.GetAsync($"http://localhost:5000/api/Variation/{variationId}");
        return response.IsSuccessStatusCode;
    }
}
