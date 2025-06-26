namespace ShopOrderMicroservice.DTOs
{
    public class ShopOrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public int PaymentId { get; set; }
        public int ShippingAddressId { get; set; }
        public int ShippingTypeId { get; set; }
        public decimal OrderTotal { get; set; }
        public int OrderStatusId { get; set; }
    }
}
