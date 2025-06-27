using Variation_OptionMicroservice.Service.DTOs;

namespace Variation_OptionMicroservice.Service.Interfaces
{
    public interface IVariationOptionService
    {
        Task<IEnumerable<VariationOptionDto>> GetAllOptionsAsync();
        Task<VariationOptionDto?> GetOptionByIdAsync(int id);
        Task<IEnumerable<VariationOptionDto>> GetOptionsByVariationIdAsync(int variationId);
        Task<VariationOptionDto> CreateOptionAsync(CreateVariationOptionDto createDto);
        Task<bool> UpdateOptionAsync(UpdateVariationOptionDto updateDto);
        Task<bool> DeleteOptionAsync(int id);
    }
}