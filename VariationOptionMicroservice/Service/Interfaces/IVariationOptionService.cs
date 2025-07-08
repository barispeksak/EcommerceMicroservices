using VariationOptionMicroservice.Service.DTOs;

namespace VariationOptionMicroservice.Service.Interfaces
{
    public interface IVariationOptionService
    {
        Task<IEnumerable<VariationOptionDto>> GetAllAsync();
        Task<VariationOptionDto> GetAsync(int id);
        Task<VariationOptionDto> CreateAsync(CreateVariationOptionDto createDto);
        Task<bool> UpdateAsync(int id, UpdateVariationOptionDto updateDto);
        Task<bool> DeleteAsync(int id);
    }
}