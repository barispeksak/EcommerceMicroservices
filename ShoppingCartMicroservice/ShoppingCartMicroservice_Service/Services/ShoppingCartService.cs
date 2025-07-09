using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ShoppingCartMicroservice_Service.Services;

namespace ShoppingCartMicroservice_Service.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IDatabase _redis;
        private readonly ProductItemClient _itemClient;
        private readonly ProductClient _productClient;

        public ShoppingCartService(
            IConnectionMultiplexer redis,
            ProductItemClient itemClient,
            ProductClient productClient)
        {
            _redis = redis.GetDatabase();
            _itemClient = itemClient;
            _productClient = productClient;
        }

        private static string CartKey(string userId) => $"cart:{userId}";

        private async Task<List<ShoppingCartItemDto>> LoadCartAsync(string userId)
        {
            var json = await _redis.StringGetAsync(CartKey(userId));
            return json.IsNullOrEmpty
                ? new List<ShoppingCartItemDto>()
                : JsonConvert.DeserializeObject<List<ShoppingCartItemDto>>(json) ?? new List<ShoppingCartItemDto>();
        }

        private Task SaveCartAsync(string userId, List<ShoppingCartItemDto> cart)
            => _redis.StringSetAsync(CartKey(userId), JsonConvert.SerializeObject(cart), TimeSpan.FromHours(1));

        private async Task<ProductItemDto> FetchProductItemAsync(int productItemId)
            => await _itemClient.GetByIdAsync(productItemId)
               ?? throw new Exception("Ürün bulunamadı!");

        public async Task AddOrUpdateItemAsync(string userId, CreateShoppingCartDto dto)
        {
            if (dto.Quantity < 1)
                throw new ArgumentException("Quantity pozitif olmalı");

            var productItem = await FetchProductItemAsync(dto.ProductItemId);

            if (dto.Quantity > productItem.QuantityInStock)
                throw new Exception("Stokta yeterli ürün yok!");

            var cart = await LoadCartAsync(userId);
            var item = cart.FirstOrDefault(i => i.Id == dto.ProductItemId);

            if (item == null)
            {
                cart.Add(new ShoppingCartItemDto
                {
                    Id = dto.ProductItemId,
                    Quantity = dto.Quantity,
                    Price = productItem.Price
                });
            }
            else
            {
                item.Quantity = dto.Quantity;
                item.Price = productItem.Price;
            }

            await SaveCartAsync(userId, cart);
        }

        public async Task RemoveItemAsync(string userId, int productItemId)
        {
            var cart = await LoadCartAsync(userId);
            cart.RemoveAll(i => i.Id == productItemId);
            await SaveCartAsync(userId, cart);
        }

        public async Task ClearAsync(string userId)
            => await _redis.KeyDeleteAsync(CartKey(userId));

        public async Task<List<CartItemDetailsDto>> GetCartDetailsForUser(string userId)
        {
            var cart = await LoadCartAsync(userId);
            if (!cart.Any()) return new List<CartItemDetailsDto>();

            var productItemIds = cart.Select(c => c.Id).ToList();
            var productItems = await _itemClient.GetByIdsAsync(productItemIds);
            var productItemMap = productItems.ToDictionary(pi => pi.Id);

            var productIds = productItems.Select(pi => pi.ProductId).Distinct().ToList();
            var products = await _productClient.GetByIdsAsync(productIds);
            var productMap = products.ToDictionary(p => p.Id);

            var details = new List<CartItemDetailsDto>();

            foreach (var ci in cart)
            {
                if (!productItemMap.TryGetValue(ci.Id, out var pi)) continue;
                if (!productMap.TryGetValue(pi.ProductId, out var prod)) continue;

                details.Add(new CartItemDetailsDto
                {
                    Id = ci.Id,
                    Sku = pi.Sku,
                    Quantity = ci.Quantity,
                    Price = ci.Price,
                    Currency = pi.Currency,
                    Name = prod.Name,
                    Image = prod.Image,
                    TotalPrice = null,
                    TotalQuantity = null
                });
            }

            var totalPrice = details.Sum(d => d.LineTotal);
            var totalQty = details.Sum(d => d.Quantity);

            details.Add(new CartItemDetailsDto
            {
                Id = 0,
                Sku = "TOTAL",
                Quantity = 0,
                Price = 0,
                Currency = details.First().Currency,
                Name = null,
                Image = null,
                TotalPrice = totalPrice,
                TotalQuantity = totalQty
            });

            return details;
        }
    }
}
