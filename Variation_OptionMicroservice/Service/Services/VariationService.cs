
using System.Text.Json;
using Variation_OptionMicroservice.Service.DTOs;
using Variation_OptionMicroservice.Service.Interfaces;

namespace Variation_OptionMicroservice.Service.Services
{
    public class VariationService : IVariationService
    {
        private readonly HttpClient _httpClient;

        public VariationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<VariationDto> GetVariationByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"http://localhost:5001/api/variation/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<VariationDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }
    }
}
