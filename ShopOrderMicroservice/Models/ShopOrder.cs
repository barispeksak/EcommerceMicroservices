using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopOrderMicroservice.Models
{
    [Table("shop_order")]
    public class ShopOrder
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; }

        [Column("payment_type_id")]
        public int PaymentTypeId { get; set; }

        [Column("shipping_address_id")]
        public int ShippingAddressId { get; set; }

        [Column("shipping_type_id")]
        public int ShippingTypeId { get; set; }

        [Column("order_total")]
        public decimal OrderTotal { get; set; }
    }
}
