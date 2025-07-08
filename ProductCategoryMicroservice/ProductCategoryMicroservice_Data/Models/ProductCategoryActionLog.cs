using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductCategoryMicroservice_Data.Models;

public class ProductCategoryActionLog
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Action { get; set; }
    public string? Level { get; set; }        // "Info", "Error", "Warn" vs.
    public string? Message { get; set; }
    public string? CorrelationId { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? ProductCategoryId { get; set; }
    public string? Name { get; set; }
    public string? ParentCategoryId { get; set; }
    public DateTime? UserDob { get; set; }
    public BsonDocument? Description { get; set; }
}

