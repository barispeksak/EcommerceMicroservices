// using PaymentTypeMicroservice.Models;
using PaymentTypeMicroservice.Entities;

namespace PaymentTypeMicroservice.Data.Repositories
{
    public interface IPaymentTypeRepository
    {
        Task<IEnumerable<PaymentType>> GetAllAsync();
        Task<PaymentType?> GetByIdAsync(int id);
        Task<PaymentType> AddAsync(PaymentType entity);
        Task<bool> UpdateAsync(PaymentType PaymentType);
        Task<bool> DeleteAsync(int id);
    }
}
