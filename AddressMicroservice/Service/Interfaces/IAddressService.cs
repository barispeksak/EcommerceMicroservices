using AddressMicroservice.Service.DTOs;

namespace AddressMicroservice.Service.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetAllAsync();
        Task<AddressDto?> GetByIdAsync(int id);
        Task<AddressDto> CreateAsync(CreateAddressDto createAddressDto);
        Task<AddressDto?> UpdateAsync(int id, UpdateAddressDto updateAddressDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}