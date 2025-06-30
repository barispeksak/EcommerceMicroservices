using VariationMicroservice.Service.DTOs;

namespace VariationMicroservice.Service.Interfaces
{
    public interface IVariationService
    {
        Task<IEnumerable<VariationDto>> GetAllAsync();
        Task<VariationDto> GetAsync(int id);
        Task<VariationDto> CreateAsync(CreateVariationDto createDto);
        Task<bool> UpdateAsync(int id, UpdateVariationDto updateDto);
        Task<bool> DeleteAsync(int id);
    }
}