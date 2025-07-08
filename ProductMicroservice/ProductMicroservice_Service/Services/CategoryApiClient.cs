public class CategoryApiClient
{
    private readonly HttpClient _client;
    public CategoryApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> CategoryExists(int categoryId)
    {
        var response = await _client.GetAsync($"http://productcategorymicroservice:8080/api/productcategory/{categoryId}");
        return response.IsSuccessStatusCode;
    }
}
