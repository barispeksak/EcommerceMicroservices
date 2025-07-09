// Services/Interfaces/IPaymentTypeService.cs
using PaymentTypeMicroservice.Data.Dtos;

namespace PaymentTypeMicroservice.Services.Interfaces
{
    public interface IPaymentTypeService
    {
        Task<IEnumerable<PaymentTypeDto>> GetAllAsync();
        Task<PaymentTypeDto?> GetByIdAsync(int id);
        Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeDto dto);
        Task<bool> UpdateAsync(UpdatePaymentTypeDto dto);
        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsByNameAsync(string typeName);

    }
}
