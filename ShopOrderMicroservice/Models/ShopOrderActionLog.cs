using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShopOrderMicroservice.Models
{
    public class ShopOrderActionLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }

        public string CorrelationId { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }  // "Success" | "Fail"

        public string Message { get; set; }
        
        public string? PerformedByEmail { get; set; }  // opsiyonel, kullanıcı oturumu varsa kullanılır

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public BsonDocument Description { get; set; } = new BsonDocument();

    }
}
