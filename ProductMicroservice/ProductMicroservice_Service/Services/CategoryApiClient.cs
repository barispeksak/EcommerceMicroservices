public class CategoryApiClient
{
    private readonly HttpClient _client;
    public CategoryApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> CategoryExists(int categoryId)
    {
        var response = await _client.GetAsync($"http://localhost:5220/api/Categories/{categoryId}"); // Kategori API adresin
        return response.IsSuccessStatusCode;
    }
}
