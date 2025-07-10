using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderStatusMicroservice.Models
{
    public class OrderStatusActionLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? CorrelationId { get; set; }

        public string? Action { get; set; }

        public string? Status { get; set; } // "Success", "Fail" vb.

        public string? Message { get; set; }

        public string? PerformedByEmail { get; set; } // kullanıcı maili, opsiyonel

        public BsonDocument? Description { get; set; } // Ekstra log verisi (istek/gelen data/detaylar)
    }
}
