using AutoMapper;
using OrderStatusMicroservice.Data.Dtos;
using OrderStatusMicroservice.Data.Repositories;
using OrderStatusMicroservice.Entities;
using OrderStatusMicroservice.Services.Interfaces;
using System.Net.Http;

namespace OrderStatusMicroservice.Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private readonly IOrderStatusRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderStatusService(
            IOrderStatusRepository repository,
            IMapper mapper,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<OrderStatusDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderStatusDto>>(list);
        }

        public async Task<OrderStatusDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<OrderStatusDto>(entity);
        }

        public async Task<OrderStatusDto> CreateAsync(CreateOrderStatusDto dto)
        {
            //ShopOrderId geçerli mi diye kontrol
            var client = _httpClientFactory.CreateClient("ShopOrderService");
            var response = await client.GetAsync($"api/shoporder/{dto.ShopOrderId}");

            if (!response.IsSuccessStatusCode)
                throw new ArgumentException($"ShopOrderId {dto.ShopOrderId} geçerli değil.");

            var entity = _mapper.Map<OrderStatus>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<OrderStatusDto>(created);
        }

        public async Task<bool> UpdateAsync(UpdateOrderStatusDto dto)
        {
            //ShopOrderId kontrolü
            var client = _httpClientFactory.CreateClient("ShopOrderService");
            var response = await client.GetAsync($"api/shoporder/{dto.ShopOrderId}");

            if (!response.IsSuccessStatusCode)
                throw new ArgumentException($"ShopOrderId {dto.ShopOrderId} geçerli değil.");

            var entity = _mapper.Map<OrderStatus>(dto);
            return await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
