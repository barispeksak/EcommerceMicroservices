using ShoppingCartMicroservice_Api.Models;
using ShoppingCartMicroservice_Service.DTOs; 
using StackExchange.Redis;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ShoppingCartMicroservice_Api.Storage
{
    public class RedisStockRepository : IStockRepository
    {
        private readonly IDatabase _redis;
        private readonly ProductItemClient _productItemClient;
        private readonly ILogger<RedisStockRepository> _logger;

        public RedisStockRepository(
            IConnectionMultiplexer redis,
            ProductItemClient productItemClient,
            ILogger<RedisStockRepository> logger)
        {
            _redis = redis.GetDatabase();
            _productItemClient = productItemClient; // ✅ Düzeltildi
            _logger = logger;
        }

        public async Task<IReadOnlyList<CartItem>> GetCartItemsAsync(Guid cartId)
        {
            var key = $"cart:{cartId}";
            var json = await _redis.StringGetAsync(key);
            if (json.IsNullOrEmpty)
            {
                _logger.LogWarning("Sepet bulunamadı. CartId: {CartId}", cartId);
                return Array.Empty<CartItem>();
            }
            var items = JsonConvert.DeserializeObject<List<CartItem>>(json) ?? new List<CartItem>();
            return items;
        }

        public async Task<bool> TryReserveAsync(IEnumerable<CartItem> items)
        {
            var itemList = items.ToList();
            if (!itemList.Any())
            {
                _logger.LogWarning("Rezerve edilecek ürün yok.");
                return false;
            }

            foreach (var item in itemList)
            {
                // ✅ Typed client kullan - ProductItemClient direkt kullanılıyor
                var productItem = await _productItemClient.GetByIdAsync(item.ProductId);
                
                if (productItem == null)
                {
                    _logger.LogWarning("Ürün bulunamadı. ProductItemId: {ProductId}", item.ProductId);
                    return false;
                }

                if (productItem.QuantityInStock < item.Quantity)
                {
                    _logger.LogWarning("Yeterli stok yok. ProductItemId: {ProductId}, İstenen: {Istenen}, Stok: {Stok}",
                        item.ProductId, item.Quantity, productItem.QuantityInStock);
                    return false;
                }
            }

            // Gerçek uygulamada burada stok düşme işlemi yapılmalı (örneğin başka bir endpoint'e PATCH/POST atılır)
            // Şimdilik sadece log atalım:
            foreach (var item in itemList)
            {
                _logger.LogInformation("Stok rezerve edildi (simülasyon). ProductItemId: {ProductId}, Quantity: {Quantity}",
                    item.ProductId, item.Quantity);
            }

            return true;
        }

        public async Task ReleaseReservationAsync(Guid cartId)
        {
            // Gerçek uygulamada burada rezerve edilen stokları geri bırakacak bir endpoint çağrısı yapılmalı.
            // Şimdilik sadece log atalım:
            _logger.LogInformation("Stok rezervasyonu geri bırakıldı (simülasyon). CartId: {CartId}", cartId);
            await Task.CompletedTask;
        }
    }
}