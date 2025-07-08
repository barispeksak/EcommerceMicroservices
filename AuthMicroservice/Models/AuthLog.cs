using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Serilog.Context;


public class AuthLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string CorrelationId { get; set; }   // YENİ!
    public string Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public BsonDocument? Description { get; set; }

}
