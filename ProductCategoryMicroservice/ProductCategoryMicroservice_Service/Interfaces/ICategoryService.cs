using System.Collections.Generic;
using System.Threading.Tasks;
using ProductCategoryMicroservice_Service.DTOs;   // DTO’ları daha sonra kullanacağız

namespace ProductCategoryMicroservice_Service.Interfaces
{
    /// <summary>
    /// Category iş mantığına ait sözleşme.
    /// (CRUD + ek sorgular gerektiğinde buraya eklenir.)
    /// </summary>
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?>             GetAsync(int id);
        Task<CategoryDto>              CreateAsync(CreateCategoryDto dto);
        Task                           UpdateAsync(int id, UpdateCategoryDto dto);
        Task                           DeleteAsync(int id);
    }
}
