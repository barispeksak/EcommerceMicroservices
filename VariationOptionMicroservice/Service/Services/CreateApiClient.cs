namespace VariationOptionMicroservice.Service.Services;

public class CategoryApiClient
{
    private readonly HttpClient _client;
    public CategoryApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> VariationExists(int variationId)
    {
        var response = await _client.GetAsync($"http://variationmicroservice:8080/api/Variation/{variationId}");
        return response.IsSuccessStatusCode;
    }
}
