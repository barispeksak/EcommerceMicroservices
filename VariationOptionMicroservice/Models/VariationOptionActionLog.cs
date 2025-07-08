using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VariationOptionMicroservice.Models;

public class VariationOptionActionLog
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Action { get; set; }
    public string? Level { get; set; }        // "Info", "Error", "Warn" vs.
    public string? Message { get; set; }
    public string? CorrelationId { get; set; }
    public string? PerformedById { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? PerformedByName { get; set; }
    public string? VariationId { get; set; }
    public string? Value { get; set; }
}

