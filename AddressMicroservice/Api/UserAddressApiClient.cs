using System.Net.Http;
using System.Threading.Tasks;

namespace AddressMicroservice.Api
{
    public class UserAddressApiClient : IUserAddressApiClient
    {
        private readonly HttpClient _httpClient;

        public UserAddressApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Example method stub
        // public async Task<UserAddressDto> GetUserAddressAsync(Guid userId)
        // {
        //     var response = await _httpClient.GetAsync($"/api/useraddress/{userId}");
        //     response.EnsureSuccessStatusCode();
        //     return await response.Content.ReadFromJsonAsync<UserAddressDto>();
        // }
    }
}