using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace ShoppingCartMicroservice_Service.Models;

public class ShoppingCartActionLog
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
    public string? CartId { get; set; }
    public string? ProductId { get; set; }
    public int? Quantity { get; set; }
    public decimal? LineTotal { get; set; }
    public decimal? CartTotal { get; set; }
    public BsonDocument? Description { get; set; }


}
