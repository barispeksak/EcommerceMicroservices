namespace ShopOrderMicroservice.Data.Dtos
{
    public class UpdateShopOrderDto
    {
        public int Id { get; set; } // PUT için şart
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public int PaymentTypeId { get; set; }
        public int ShippingAddressId { get; set; }
        public int ShippingTypeId { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
