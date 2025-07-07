using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApiGateway.Models
{
    public class RequestLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Method { get; set; }

        public string? Path { get; set; }

        public int StatusCode { get; set; }

        public string? UserId { get; set; }

        public string? IP { get; set; }

        public string? ServiceName { get; set; }

        public long ResponseTimeMs { get; set; }
    }
}
