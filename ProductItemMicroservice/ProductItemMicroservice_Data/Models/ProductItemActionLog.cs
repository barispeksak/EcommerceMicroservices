using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductItemMicroservice_Data.Models;

public class ProductItemActionLog
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
    public string? Sku { get; set; }
    public string? QuantityInStock { get; set; }
    public string? Price { get; set; }
    public string? Currency { get; set; }
    public string? ProductId { get; set; }
    
}

