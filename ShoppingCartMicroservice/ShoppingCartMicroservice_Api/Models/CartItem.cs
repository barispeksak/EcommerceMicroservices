namespace ShoppingCartMicroservice_Api.Models;

public record CartItem(Guid CartId, int ProductId, int Quantity);