using System.ComponentModel.DataAnnotations;

namespace ShopOrderMicroservice.Data.Dtos
{
    public class CreateShopOrderDto
    {
        public int UserId { get; set; }
        public int PaymentTypeId { get; set; }
        public int ShippingAddressId { get; set; }
        public int ShippingTypeId { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
