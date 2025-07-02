using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
// ProductClient adını senin client dosyanla eşleştir!
using ShoppingCartMicroservice_Service.Services; // ProductClient burada olmalı
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartMicroservice_Service.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IDatabase _redis;
        private readonly ProductClient _productClient; // Yeni client

        public ShoppingCartService(IConnectionMultiplexer redis,
                                   ProductClient productClient)
        {
            _redis        = redis.GetDatabase();
            _productClient = productClient;
        }

        private static string CartKey(string userId) => $"cart:{userId}";

        private async Task<List<ShoppingCartItemDto>> LoadCartRawAsync(string userId)
        {
            var json = await _redis.StringGetAsync(CartKey(userId));
            return json.IsNullOrEmpty
                ? new()
                : JsonConvert.DeserializeObject<List<ShoppingCartItemDto>>(json) ?? new();
        }

        public async Task AddOrUpdateItemAsync(string userId, CreateShoppingCartDto dto)
        {
            // 1. Önce ProductItem çek (stok, fiyat, productId)
            var productItem = await _productClient.GetProductItemByIdAsync(dto.ProductItemId)
                              ?? throw new Exception("Ürün bulunamadı!");

            if (dto.Quantity > productItem.QuantityInStock)
                throw new Exception("Stokta yeterli ürün yok!");

            var items    = await LoadCartRawAsync(userId);
            var existing = items.FirstOrDefault(x => x.Id == dto.ProductItemId);
            var mevcut   = existing?.Quantity ?? 0;

            if (mevcut + dto.Quantity > productItem.QuantityInStock)
                throw new Exception($"Sepetteki toplam miktar stoktan ({productItem.QuantityInStock}) fazla olamaz!");

            if (existing != null)
                existing.Quantity += dto.Quantity;
            else
                items.Add(new ShoppingCartItemDto { Id = dto.ProductItemId, Quantity = dto.Quantity });

            await _redis.StringSetAsync(CartKey(userId), JsonConvert.SerializeObject(items),TimeSpan.FromMinutes(10));
        }

        public async Task RemoveItemAsync(string userId, int productItemId)
        {
            var items = await LoadCartRawAsync(userId);
            var target = items.FirstOrDefault(x => x.Id == productItemId)
                      ?? throw new Exception("Sepette böyle bir ürün yok!");

            items.Remove(target);
            await _redis.StringSetAsync(CartKey(userId), JsonConvert.SerializeObject(items),TimeSpan.FromMinutes(10));
        }

        public async Task ClearAsync(string userId) =>
            await _redis.KeyDeleteAsync(CartKey(userId));

        public async Task<List<CartItemDetailsDto>> GetCartDetailsForUser(string userId)
        {
            var cartItems = await LoadCartRawAsync(userId);
            if (!cartItems.Any()) return new();

            // 1. Sepetteki item Id’leriyle ProductItem çek (stok, fiyat, productId, sku)
            var productItemIds = cartItems.Select(ci => ci.Id);
            var productItems   = await _productClient.GetProductItemsByIdsAsync(productItemIds);
            var itemMap        = productItems.ToDictionary(pi => pi.Id);

            // 2. Gelen ProductItem'lardan productId kümesiyle Product çek (name, image)
            var productIds = productItems.Select(pi => pi.ProductId).Distinct();
            var products   = await _productClient.GetProductsByIdsAsync(productIds);
            var prodMap    = products.ToDictionary(p => p.Id);

            // 3. Merge (ProductItem + Product)
            var result = new List<CartItemDetailsDto>();

            foreach (var ci in cartItems)
            {
                if (!itemMap.TryGetValue(ci.Id, out var pi)) continue;        // item bulunamazsa atla
                if (!prodMap.TryGetValue(pi.ProductId, out var p)) continue;  // ana ürün bulunamazsa atla

                result.Add(new CartItemDetailsDto
                {
                    Id       = ci.Id,
                    Sku      = pi.Sku,
                    Quantity = ci.Quantity,
                    Price    = pi.Price,
                    Currency = pi.Currency,
                    Name     = p.Name,
                    Image    = p.Image
                });
            }

            return result;
        }
    }
}
