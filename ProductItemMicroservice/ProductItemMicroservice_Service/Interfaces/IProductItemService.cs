using ProductItemMicroservice_Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductItemMicroservice_Service.Interfaces
{
    public interface IProductItemService
    {
        Task<ProductItemDto>  GetByIdAsync(int id);
        Task<List<ProductItemDto>> GetAllAsync();
        Task<ProductItemDto>  CreateAsync(CreateProductItemDto dto);
        Task<ProductItemDto>  UpdateAsync(int id, CreateProductItemDto dto);
        Task DeleteAsync(int id);
    }
}
