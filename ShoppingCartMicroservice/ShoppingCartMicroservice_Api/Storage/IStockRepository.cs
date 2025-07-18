using ShoppingCartMicroservice_Api.Models;

namespace ShoppingCartMicroservice_Api.Storage;

public interface IStockRepository
{
    Task<IReadOnlyList<CartItem>> GetCartItemsAsync(Guid cartId);
    Task<bool> TryReserveAsync(IEnumerable<CartItem> items);
    Task ReleaseReservationAsync(Guid cartId);
}