using AutoMapper;
using ShopOrderMicroservice.Data.Dtos;
using ShopOrderMicroservice.Models;
using ShopOrderMicroservice.Repositories;
using ShopOrderMicroservice.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;

namespace ShopOrderMicroservice.Services
{
    public class ShopOrderService : IShopOrderService
    {
        private readonly IShopOrderRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;

        public ShopOrderService(IShopOrderRepository repository, IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
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
            // HttpClient'lar
            var userClient = _httpClientFactory.CreateClient("UserService");
            var addressClient = _httpClientFactory.CreateClient("AddressService");
            var shippingClient = _httpClientFactory.CreateClient("ShippingService");
            var paymentClient = _httpClientFactory.CreateClient("PaymentService");
            var summaryClient = _httpClientFactory.CreateClient("ShoppingCartService");

            // Validasyonlar
            if (!(await userClient.GetAsync($"api/user/{dto.UserId}")).IsSuccessStatusCode)
                return null;

            if (!(await addressClient.GetAsync($"api/address/{dto.ShippingAddressId}")).IsSuccessStatusCode)
                return null;

            if (!(await shippingClient.GetAsync($"api/shippingtype/{dto.ShippingTypeId}")).IsSuccessStatusCode)
                return null;

            if (!(await paymentClient.GetAsync($"api/paymenttype/{dto.PaymentTypeId}")).IsSuccessStatusCode)
                return null;

            // ShoppingCart nesnesinden shipping fiyatını al
            var cartResponse = await summaryClient.GetFromJsonAsync<ShoppingCartDto>($"api/shoppingcart/{dto.ShopId}");
            if (cartResponse == null)
                return null;

            decimal productTotal = cartResponse.TotalPrice;

            // ShippingType nesnesinden shipping fiyatını al
            var shippingResponse = await shippingClient.GetFromJsonAsync<ShippingTypeDto>($"api/shippingtype/{dto.ShippingTypeId}");
            if (shippingResponse == null)
                return null;

            decimal shippingPrice = shippingResponse.Price;

            // Sipariş oluşturma
            var order = _mapper.Map<ShopOrder>(dto);
            order.OrderDate = DateTime.UtcNow;
            order.OrderTotal = productTotal + shippingPrice;

            await _repository.AddAsync(order);
            return _mapper.Map<ShopOrderDto>(order);
        }



        public async Task<bool> UpdateAsync(int id, UpdateShopOrderDto dto)
        {
            var userClient = _httpClientFactory.CreateClient("UserService");
            var addressClient = _httpClientFactory.CreateClient("AddressService");
            var shippingClient = _httpClientFactory.CreateClient("ShippingService");
            var paymentClient = _httpClientFactory.CreateClient("PaymentService");

            if (!(await userClient.GetAsync($"api/user/{dto.UserId}")).IsSuccessStatusCode)
                return false;

            if (!(await addressClient.GetAsync($"api/address/{dto.ShippingAddressId}")).IsSuccessStatusCode)
                return false;

            if (!(await shippingClient.GetAsync($"api/shippingtype/{dto.ShippingTypeId}")).IsSuccessStatusCode)
                return false;

            if (!(await paymentClient.GetAsync($"api/paymenttype/{dto.PaymentTypeId}")).IsSuccessStatusCode)
                return false;

            var order = _mapper.Map<ShopOrder>(dto);
            order.Id = id;

            return await _repository.UpdateAsync(order);
        }

        public async Task<bool> DeleteAsync(int id)
            => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<ShopOrderDto>> GetByUserIdAsync(int userId)
        {
            var orders = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShopOrderDto>>(orders.Where(x => x.UserId == userId));
        }

        public async Task<IEnumerable<ShopOrderDto>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var orders = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShopOrderDto>>(
                orders.Where(x => x.OrderDate >= start && x.OrderDate <= end));
        }
    }
}
