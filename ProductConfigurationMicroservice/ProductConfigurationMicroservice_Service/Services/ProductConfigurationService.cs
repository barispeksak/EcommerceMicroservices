using ProductConfigurationMicroservice_Data.Repositories;
using ProductConfigurationMicroservice_Service.DTOs;
using ProductConfigurationMicroservice_Service.Interfaces;
using ProductConfigurationMicroservice_Service.Services;   // ApiClient'larımız bu namespace'te
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductConfigurationMicroservice_Service.Services;

/// <summary>
/// Listele • Tekil Get • Sil  —
/// her çağrıda ProductItem & VariationOption servislerinden
/// SKU ile VariationOptionName’i alıp DTO’yu zenginleştirir.
/// </summary>
public class ProductConfigurationService : IProductConfigurationService
{
    private readonly IProductConfigurationRepository _repo;
    private readonly ProductItemApiClient _itemApi;
    private readonly VariationOptionApiClient _optApi;

    public ProductConfigurationService(
        IProductConfigurationRepository repo,
        ProductItemApiClient itemApi,
        VariationOptionApiClient optApi)
    {
        _repo   = repo;
        _itemApi = itemApi;
        _optApi  = optApi;
    }

    public async Task<IEnumerable<ProductConfigurationDto>> GetAllAsync(int? productItemId = null)
    {
        var entities = await _repo.GetAllAsync(productItemId);
        var list = new List<ProductConfigurationDto>();

        foreach (var pc in entities)
        {
            var (_, sku)   = await _itemApi.TryGetAsync(pc.ProductItemId);
            var (_, name)  = await _optApi.TryGetAsync(pc.VariationOptionId);

            list.Add(new ProductConfigurationDto(
                pc.Id,
                pc.ProductItemId,
                pc.VariationOptionId,
                sku,
                name));
        }
        return list;
    }

    public async Task<ProductConfigurationDto?> GetByIdAsync(int id)
    {
        var pc = await _repo.GetByIdAsync(id);
        if (pc is null) return null;

        var (_, sku)  = await _itemApi.TryGetAsync(pc.ProductItemId);
        var (_, name) = await _optApi.TryGetAsync(pc.VariationOptionId);

        return new ProductConfigurationDto(
            pc.Id,
            pc.ProductItemId,
            pc.VariationOptionId,
            sku,
            name);
    }

    public async Task DeleteAsync(int id)
    {
        var pc = await _repo.GetByIdAsync(id)
                 ?? throw new KeyNotFoundException($"Config {id} not found");

        await _repo.RemoveAsync(pc);
        await _repo.SaveAsync();
    }
}
