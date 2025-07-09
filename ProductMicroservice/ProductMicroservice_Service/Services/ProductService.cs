using AutoMapper;
using FluentValidation;
using ProductMicroservice_Data.Entities;
using ProductMicroservice_Data.Repositories;
using ProductMicroservice_Service.DTOs;
using ProductMicroservice_Service.Interfaces;

namespace ProductMicroservice_Service.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IValidator<CreateProductDto> _createVal;
    private readonly IValidator<UpdateProductDto> _updateVal;
    private readonly IMapper _mapper;
    private readonly CategoryApiClient _categoryApiClient;

    public ProductService(
        CategoryApiClient categoryApiClient,
        IProductRepository repo,
        IValidator<CreateProductDto> createVal,
        IValidator<UpdateProductDto> updateVal,
        IMapper mapper)
    {
        _categoryApiClient = categoryApiClient;
        _repo = repo;
        _createVal = createVal;
        _updateVal = updateVal;
        _mapper = mapper;
    }

    /* GET /{id} */
    public async Task<ProductDto> GetAsync(int id)
    {
        var entity = await _repo.GetAsync(id)
                     ?? throw new KeyNotFoundException($"Product {id} not found");
        return _mapper.Map<ProductDto>(entity);
    }

    /* GET / */
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(_mapper.Map<ProductDto>);
    }

    /* POST */
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var categoryExists = await _categoryApiClient.CategoryExists(dto.CategoryId);
        if (!categoryExists)
            throw new Exception("Girilen kategori bulunamadı!");
        await _createVal.ValidateAndThrowAsync(dto);
        var entity = _mapper.Map<Product>(dto);
        await _repo.AddAsync(entity);
        await _repo.SaveAsync();
        return _mapper.Map<ProductDto>(entity);
    }

    /* PUT */
    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        var entity = await _repo.GetAsync(id)
                     ?? throw new KeyNotFoundException($"Product {id} not found");

        await _updateVal.ValidateAndThrowAsync(dto);
        _mapper.Map(dto, entity);           // null olmayan alanlar uygulanır
        await _repo.SaveAsync();
        return _mapper.Map<ProductDto>(entity);
    }

    /* DELETE */
    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetAsync(id)
                     ?? throw new KeyNotFoundException($"Product {id} not found");

        await _repo.DeleteAsync(entity);
        await _repo.SaveAsync();
    }
    
    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _categoryApiClient.CategoryExists(categoryId);
    }

}
