// Data/Dtos/OrderStatusDtos.cs
namespace OrderStatusMicroservice.Data.Dtos
{
    public class CreateOrderStatusDto
    {
        public int ShopOrderId { get; set; }
        public string Status { get; set; } = null!;
        public string City { get; set; } = null!;
    }

    public class UpdateOrderStatusDto
    {
            public int Id { get; set; }
            public int ShopOrderId { get; set; }
            public string Status { get; set; } = null!;
            public string City { get; set; } = null!;
    }

    public class OrderStatusDto
    {
        public int Id { get; set; }
        public int ShopOrderId { get; set; }
        public string Status { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}
