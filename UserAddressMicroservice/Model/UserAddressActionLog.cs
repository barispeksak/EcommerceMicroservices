using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserAddressMicroservice.Models
{
    public class UserAddressActionLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }

        public string CorrelationId { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }  // "Success" | "Fail"

        public string Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public BsonDocument Description { get; set; } = new BsonDocument();

        public string? PerformedBy { get; set; }  // opsiyonel, kullanıcı oturumu varsa kullanılır
    }
}
