using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ShopOrderMicroservice.Data.Dtos;
using ShopOrderMicroservice.Data.Entities;
using ShopOrderMicroservice.Data.Repositories;
using ShopOrderMicroservice.Services.Interfaces;

namespace ShopOrderMicroservice.Services
{
    public class ShopOrderService : IShopOrderService
    {
                // ... mevcut kod ...
        private readonly IShopOrderRepository _repository;
        private readonly IMapper _mapper;

        public ShopOrderService(IShopOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShopOrderDto>> GetAllAsync()
        {
            var orders = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShopOrderDto>>(orders);
        }

        public async Task<ShopOrderDto?> GetByIdAsync(int id)
        {
            var order = await _repository.GetByIdAsync(id);
            return order == null ? null : _mapper.Map<ShopOrderDto>(order);
        }

        public async Task<ShopOrderDto?> CreateAsync(CreateShopOrderDto dto)
        {
            var entity = _mapper.Map<ShopOrderMicroservice.Data.Entities.ShopOrder>(dto);
            await _repository.AddAsync(entity);
            return _mapper.Map<ShopOrderDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateShopOrderDto dto)
        {
            var entity = _mapper.Map<ShopOrderMicroservice.Data.Entities.ShopOrder>(dto);
            entity.Id = id;
            return await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ShopOrderDto>> GetByUserIdAsync(int userId)
        {
            var orders = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ShopOrderDto>>(orders);
        }

        public async Task<IEnumerable<ShopOrderDto>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var orders = await _repository.GetByDateRangeAsync(start, end);
            return _mapper.Map<IEnumerable<ShopOrderDto>>(orders);
        }
    }
}