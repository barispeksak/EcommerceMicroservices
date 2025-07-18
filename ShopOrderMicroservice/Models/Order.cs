using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopOrderMicroservice.Models
{
    [Table("quick_orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("cart_id")]
        public Guid CartId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}