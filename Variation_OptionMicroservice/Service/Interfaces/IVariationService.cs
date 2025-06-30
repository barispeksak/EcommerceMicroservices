
using Variation_OptionMicroservice.Service.DTOs;

namespace Variation_OptionMicroservice.Service.Interfaces
{
    public interface IVariationService
    {
        Task<VariationDto> GetVariationByIdAsync(int id);
    }
}
