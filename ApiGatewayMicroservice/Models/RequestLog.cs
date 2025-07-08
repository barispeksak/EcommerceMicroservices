using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApiGateway.Models
{
    public class RequestLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? RequestPath { get; set; }

        public string? UserId { get; set; }

        public string? UserEmail { get; set; }

        public string? UserName { get; set; }

        public string? CorrelationId { get; set; }

        public string? Action { get; set; }

        public string? Message { get; set; }
    }
}
