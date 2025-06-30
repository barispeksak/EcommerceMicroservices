using ShippingTypeMicroservice.Entities;

namespace ShippingTypeMicroservice.Data.Repositories
{
    public interface IShippingTypeRepository
    {
        Task<IEnumerable<ShippingType>> GetAllAsync();
        Task<ShippingType?> GetByIdAsync(int id);
        Task<ShippingType> AddAsync(ShippingType entity);
        Task<bool> UpdateAsync(ShippingType shippingType);
        Task<bool> DeleteAsync(int id);
    }
}
