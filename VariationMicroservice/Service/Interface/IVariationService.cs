using VariationMicroservice.Service.DTOs;

namespace VariationMicroservice.Service.Interfaces
{
    public interface IVariationService
    {
        Task<IEnumerable<VariationDto>> GetAllVariationsAsync();
        Task<VariationDto?> GetVariationByIdAsync(int id);
        Task<VariationDto> CreateVariationAsync(CreateVariationDto createDto);
        Task<bool> UpdateVariationAsync(UpdateVariationDto updateDto);
        Task<bool> DeleteVariationAsync(int id);
    }
}