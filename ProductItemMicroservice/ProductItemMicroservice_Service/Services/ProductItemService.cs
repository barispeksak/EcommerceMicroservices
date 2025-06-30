using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ProductItemMicroservice_Data.Entities;
using ProductItemMicroservice_Data.Repositories;
using ProductItemMicroservice_Service.DTOs;
using ProductItemMicroservice_Service.Interfaces;

namespace ProductItemMicroservice_Service.Services
{
    /// <summary>
    /// ProductItem ile ilgili tüm iş mantığını içerir.
    /// </summary>
    public class ProductItemService : IProductItemService
    {
        private readonly IProductItemRepository _repo;
        private readonly ProductApiClient         _productApi;
        private readonly IMapper _mapper;

        public ProductItemService(
            IProductItemRepository repo,
            ProductApiClient       productApi,
            IMapper mapper)
        {
            _repo          = repo;
            _productApi = productApi;
            _mapper        = mapper;
        }

        /* ========== READ ========== */

        public async Task<ProductItemDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException("ProductItem bulunamadı.");
            return _mapper.Map<ProductItemDto>(entity);
        }

        public async Task<List<ProductItemDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<List<ProductItemDto>>(list);
        }

        /* ========== CREATE ========== */

        public async Task<ProductItemDto> CreateAsync(CreateProductItemDto dto)
        {
            // SKU benzersiz mi?
            if (await _repo.SkuExistsAsync(dto.Sku))
                throw new InvalidOperationException("Bu SKU zaten mevcut.");

            // ProductId gerçekten var mı?
            if (!await _productApi.ProductExists(dto.ProductId))
                throw new Exception("Geçersiz ProductId: Böyle bir ürün yok.");

            var entity = _mapper.Map<ProductItem>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<ProductItemDto>(entity);
        }

        /* ========== UPDATE ========== */

        public async Task<ProductItemDto> UpdateAsync(int id, CreateProductItemDto dto)
        {
            var entity = await _repo.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException("ProductItem bulunamadı.");

            // SKU başka bir kayıtta var mı?
            if (entity.Sku != dto.Sku && await _repo.SkuExistsAsync(dto.Sku))
                throw new InvalidOperationException("Bu SKU zaten mevcut.");

            // Product kontrolü
            if (!await _productApi.ProductExists(dto.ProductId))
                throw new Exception("Geçersiz ProductId: Böyle bir ürün yok.");

            // Alanları güncelle
            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);

            return _mapper.Map<ProductItemDto>(entity);
        }

        /* ========== DELETE ========== */

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException("ProductItem bulunamadı.");
            await _repo.DeleteAsync(entity);
        }
    }
}
