using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserActionLog
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Action { get; set; }
    public string? Level { get; set; }        // "Info", "Error", "Warn" vs.
    public string? Message { get; set; }      // Kısa summary
    public string? CorrelationId { get; set; }
    public string? PerformedById { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? PerformedByName { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? UserPhone { get; set; }
    public DateTime? UserDob { get; set; }
    public string? Description { get; set; } 
}

