using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShippingTypeMicroservice.Models
{
    public class ShippingActionLog
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string CorrelationId { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }  // "Success" | "Fail"

        public string Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public BsonDocument Description { get; set; } = new BsonDocument();

        public string? PerformedBy { get; set; }  // opsiyonel, kullanıcı oturumu varsa kullanılır
    }
}
