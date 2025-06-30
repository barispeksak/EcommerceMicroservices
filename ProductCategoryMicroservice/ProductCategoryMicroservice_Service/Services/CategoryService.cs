using AutoMapper;
using ProductCategoryMicroservice_Data.Entities;
using ProductCategoryMicroservice_Data.Repositories;
using ProductCategoryMicroservice_Service.DTOs;
using ProductCategoryMicroservice_Service.Interfaces;

namespace ProductCategoryMicroservice_Service.Services;

/// <summary>
/// İş mantığı: CRUD + haritalama + repository çağrıları  
/// (SaveAsync deseni ProductMicroservice’tekiyle aynı)
/// </summary>
public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IMapper              _mapper;

    public CategoryService(ICategoryRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    /* -----------------------------------------------------------
     * READ
     * --------------------------------------------------------- */
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(entities);
    }

    public async Task<CategoryDto?> GetAsync(int id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<CategoryDto>(entity);
    }

    /* -----------------------------------------------------------
     * CREATE
     * --------------------------------------------------------- */
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {

        // --- Duplicate kontrolü ---
        bool exists = await _repo.AnyAsync(dto.CategoryName, dto.ParentCategoryId);

        if (exists)
            throw new Exception("Aynı isimde kategori zaten mevcut!"); // İstersen özel exception fırlat

        var entity = _mapper.Map<Category>(dto);
        await _repo.AddAsync(entity);
        await _repo.SaveAsync();

        return _mapper.Map<CategoryDto>(entity);
    }

    /* -----------------------------------------------------------
     * UPDATE
     * --------------------------------------------------------- */
    public async Task UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var updated = _mapper.Map<Category>(dto);
        await _repo.UpdateAsync(id, updated);
        await _repo.SaveAsync();
    }

    /* -----------------------------------------------------------
     * DELETE
     * --------------------------------------------------------- */
    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        await _repo.SaveAsync();
    }
}
