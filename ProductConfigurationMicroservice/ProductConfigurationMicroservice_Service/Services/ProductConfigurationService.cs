using ProductConfigurationMicroservice_Data.Entities;
using ProductConfigurationMicroservice_Data.Repositories;
using ProductConfigurationMicroservice_Service.DTOs;
using ProductConfigurationMicroservice_Service.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductConfigurationMicroservice_Service.Services;

/// <summary>
/// Konfigürasyon CRUD’u + SKU & OptionName zenginleştirmesi.
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
        _repo = repo;
        _itemApi = itemApi;
        _optApi = optApi;
    }

    /* ───────────── LISTE ───────────── */

    public async Task<IEnumerable<ProductConfigurationDto>> GetAllAsync(
        int[]? productItemIds,
        int[]? variationOptionIds)
    {
        var entities = await _repo.GetAllAsync(productItemIds, variationOptionIds);
        var list = new List<ProductConfigurationDto>();

        foreach (var pc in entities)
        {
            var (_, sku) = await _itemApi.TryGetAsync(pc.ProductItemId);
            var (_, name) = await _optApi.TryGetAsync(pc.VariationOptionId);

            list.Add(new ProductConfigurationDto(
                pc.Id,
                pc.ProductItemId,
                pc.VariationOptionId,
                sku,
                name));
        }
        return list;
    }

    /* ───────────── TEKIL GET ───────────── */

    public async Task<ProductConfigurationDto?> GetByIdAsync(int id)
    {
        var pc = await _repo.GetByIdAsync(id);
        if (pc is null) return null;

        var (_, sku) = await _itemApi.TryGetAsync(pc.ProductItemId);
        var (_, name) = await _optApi.TryGetAsync(pc.VariationOptionId);

        return new ProductConfigurationDto(
            pc.Id,
            pc.ProductItemId,
            pc.VariationOptionId,
            sku,
            name);
    }

    /* ───────────── CREATE ───────────── */

    public async Task<ProductConfigurationDto> AddAsync(CreateProductConfigurationDto dto)
    {
        var entity = new ProductConfiguration
        {
            ProductItemId = dto.ProductItemId,
            VariationOptionId = dto.VariationOptionId
        };
        await _repo.AddAsync(entity);
        await _repo.SaveAsync();

        var (_, sku) = await _itemApi.TryGetAsync(entity.ProductItemId);
        var (_, name) = await _optApi.TryGetAsync(entity.VariationOptionId);

        return new ProductConfigurationDto(
            entity.Id,
            entity.ProductItemId,
            entity.VariationOptionId,
            sku,
            name);
    }

    /* ───────────── UPDATE ───────────── */

    public async Task UpdateAsync(UpdateProductConfigurationDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id)
                     ?? throw new KeyNotFoundException($"Config {dto.Id} not found");

        entity.ProductItemId = dto.ProductItemId;
        entity.VariationOptionId = dto.VariationOptionId;

        await _repo.UpdateAsync(entity);
        await _repo.SaveAsync();
    }

    /* ───────────── DELETE ───────────── */

    public async Task DeleteAsync(int id)
    {
        var pc = await _repo.GetByIdAsync(id)
                 ?? throw new KeyNotFoundException($"Config {id} not found");

        await _repo.RemoveAsync(pc);
        await _repo.SaveAsync();
    }
    
    public async Task<(bool exists, string sku)> ProductItemExistsAsync(int productItemId)
        => await _itemApi.TryGetAsync(productItemId);

    public async Task<(bool exists, string value)> VariationOptionExistsAsync(int optionId)
        => await _optApi.TryGetAsync(optionId);
}
