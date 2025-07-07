using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ShoppingCartMicroservice_Service.Services
{
    /// <summary>
    /// Redis tabanlı sepet servisinin TAMAMEN güncellenmiş sürümü.
    /// - ShoppingCartItemDto artık Price + get-only LineTotal içerir.
    /// - CartItemDetailsDto ekstra TotalPrice & TotalQuantity alanlarına sahiptir.
    /// - Bu servis: ekleme, güncelleme, silme, temizleme ve detay sorgusu yapar.
    /// - Detay sorgusu listenin sonuna özet satırı ekler (TotalPrice & TotalQuantity).
    /// </summary>
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IDatabase _redis;
        private readonly ProductClient _productClient; // dış API client

        public ShoppingCartService(IConnectionMultiplexer redis, ProductClient productClient)
        {
            _redis = redis.GetDatabase();
            _productClient = productClient;
        }

        /* ============================================================ */
        /*  Private Helpers                                             */
        /* ============================================================ */

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
            => await _productClient.GetProductItemByIdAsync(productItemId)
                  ?? throw new Exception("Ürün bulunamadı!");

        /* ============================================================ */
        /*  Public API (IShoppingCartService)                           */
        /* ============================================================ */

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
                    Id       = dto.ProductItemId,
                    Quantity = dto.Quantity,
                    Price    = productItem.Price // LineTotal getter'ı otomatik
                });
            }
            else
            {
                item.Quantity = dto.Quantity;
                item.Price    = productItem.Price; // fiyat güncellemesi
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

            // 1️⃣ ProductItem & Product verilerini topla
            var productItemIds = cart.Select(c => c.Id).ToList();
            var productItems = await _productClient.GetProductItemsByIdsAsync(productItemIds);
            var productItemMap = productItems.ToDictionary(pi => pi.Id);

            var productIds = productItems.Select(pi => pi.ProductId).Distinct().ToList();
            var products = await _productClient.GetProductsByIdsAsync(productIds);
            var productMap = products.ToDictionary(p => p.Id);

            // 2️⃣ DTO listesi oluştur
            var details = new List<CartItemDetailsDto>();

            foreach (var ci in cart)
            {
                if (!productItemMap.TryGetValue(ci.Id, out var pi)) continue;
                if (!productMap.TryGetValue(pi.ProductId, out var prod)) continue;

                details.Add(new CartItemDetailsDto
                {
                    Id            = ci.Id,
                    Sku           = pi.Sku,
                    Quantity      = ci.Quantity,
                    Price         = ci.Price,
                    Currency      = pi.Currency,
                    Name          = prod.Name,
                    Image         = prod.Image,
                    TotalPrice    = null,
                    TotalQuantity = null
                });
            }

            // 3️⃣ Özet satırı ekle
            var totalPrice = details.Sum(d => d.LineTotal);
            var totalQty   = details.Sum(d => d.Quantity);

            details.Add(new CartItemDetailsDto
            {
                Id            = 0,
                Sku           = "TOTAL",
                Quantity      = 0,
                Price         = 0,
                Currency      = details.First().Currency,
                Name          = null,
                Image         = null,
                TotalPrice    = totalPrice,
                TotalQuantity = totalQty
            });

            return details;
        }
    }
}
