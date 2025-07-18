using ShoppingCartMicroservice_Api.Models;
using ShoppingCartMicroservice_Service.Interfaces;
using StackExchange.Redis;
using Newtonsoft.Json;
using ShoppingCartMicroservice_Service.DTOs;

namespace ShoppingCartMicroservice_Api.Storage;

public class InMemoryStockRepository : IStockRepository
{
    // ProductId → adet
    private readonly Dictionary<int, int> _inventory = new();
    // CartId → rezerve kalemleri
    private readonly Dictionary<Guid, List<CartItem>> _reservations = new();
    private readonly object _lock = new();
    
    // Add dependencies for real cart data access
    private readonly IDatabase _redis;
    private readonly ILogger<InMemoryStockRepository> _logger;

    public InMemoryStockRepository(IConnectionMultiplexer redis, ILogger<InMemoryStockRepository> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
        
        // Demo amaçlı 5 ürün stokluyoruz
        for (var i = 1; i <= 5; i++)
            _inventory[i] = 10;
    }

    public async Task<IReadOnlyList<CartItem>> GetCartItemsAsync(Guid cartId)
    {
        try
        {
            // CRITICAL: Map CartId (Guid) to UserId (string)
            // For now, we'll convert CartId to string to find Redis cart
            var userId = cartId.ToString();
            var cartKey = $"cart:{userId}";
            
            _logger.LogInformation("Getting cart items for CartId: {CartId}, Redis key: {CartKey}", cartId, cartKey);
            
            var json = await _redis.StringGetAsync(cartKey);
            if (json.IsNullOrEmpty)
            {
                _logger.LogWarning("No cart found for CartId: {CartId}", cartId);
                return Array.Empty<CartItem>();
            }

            var cartItems = JsonConvert.DeserializeObject<List<ShoppingCartItemDto>>(json) ?? new List<ShoppingCartItemDto>();
            
            // Convert ShoppingCartItemDto to CartItem
            var result = cartItems.Select(item => new CartItem(
                cartId,
                item.Id, // ProductId is now int, no conversion needed
                item.Quantity
            )).ToList();

            _logger.LogInformation("Found {Count} items in cart {CartId}", result.Count, cartId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart items for CartId: {CartId}", cartId);
            return Array.Empty<CartItem>();
        }
    }

    public Task<bool> TryReserveAsync(IEnumerable<CartItem> items)
    {
        lock (_lock)
        {
            var list = items.ToList();
            _logger.LogInformation("Attempting to reserve stock for {Count} items", list.Count);
            
            // Handle empty cart case
            if (list.Count == 0)
            {
                _logger.LogWarning("No items to reserve - empty cart");
                return Task.FromResult(false); // Return false for empty cart
            }
            
            // Yeterli mi?
            foreach (var item in items)
            {
                if (!_inventory.ContainsKey(item.ProductId))
                {
                    _inventory[item.ProductId] = 100; // Default stock
                }

                if (_inventory[item.ProductId] < item.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for ProductId: {ProductId}. Available: {Available}, Requested: {Requested}",
                        item.ProductId, _inventory[item.ProductId], item.Quantity);
                    return Task.FromResult(false);
                }

                _inventory[item.ProductId] -= item.Quantity;
                _logger.LogInformation("Reserved {Quantity} units of ProductId: {ProductId}", item.Quantity, item.ProductId);
            }

            // Now safe to access list[0] since we checked Count > 0
            _reservations[list[0].CartId] = list;
            _logger.LogInformation("Stock reservation successful for CartId: {CartId}", list[0].CartId);
            return Task.FromResult(true);
        }
    }

    public Task ReleaseReservationAsync(Guid cartId)
    {
        lock (_lock)
        {
            if (_reservations.Remove(cartId, out var items))
            {
                foreach (var it in items)
                {
                    _inventory[it.ProductId] += it.Quantity;
                    _logger.LogInformation("Released {Quantity} units of ProductId: {ProductId}", it.Quantity, it.ProductId);
                }
                _logger.LogInformation("Stock reservation released for CartId: {CartId}", cartId);
            }
            else
            {
                _logger.LogWarning("No reservation found to release for CartId: {CartId}", cartId);
            }
        }
        return Task.CompletedTask;
    }
}