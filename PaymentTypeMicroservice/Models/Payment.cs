using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentTypeMicroservice.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("cart_id")]
        public Guid CartId { get; set; }
        
        [Column("order_id")]
        public Guid OrderId { get; set; }
        
        [Column("amount")]
        public decimal Amount { get; set; }
        
        [Column("payment_type_id")]
        public int PaymentTypeId { get; set; }
        
        [Column("status")]
        public string Status { get; set; } = string.Empty;
        
        [Column("transaction_id")]
        public string? TransactionId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("processed_at")]
        public DateTime? ProcessedAt { get; set; }
        
        [Column("correlation_id")]
        public Guid CorrelationId { get; set; }
    }
    
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled,
        Refunded
    }
}