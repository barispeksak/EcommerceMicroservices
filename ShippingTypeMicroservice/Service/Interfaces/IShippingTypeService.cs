// Services/Interfaces/IShippingTypeService.cs
using ShippingTypeMicroservice.Data.Dtos;

namespace ShippingTypeMicroservice.Services.Interfaces
{
    public interface IShippingTypeService
    {
        Task<IEnumerable<ShippingTypeDto>> GetAllAsync();
        Task<ShippingTypeDto?> GetByIdAsync(int id);
        Task<ShippingTypeDto> CreateAsync(CreateShippingTypeDto dto);
        Task<bool> UpdateAsync(UpdateShippingTypeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
