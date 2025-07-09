using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShoppingCartMicroservice_Service.Models;

namespace ShoppingCartMicroservice_Service.Services
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Database        { get; set; } = string.Empty;
        public string CollectionName  { get; set; } = "ShoppingCartLogs";
    }

    public class ShoppingCartActionLogger
    {
        private readonly IMongoCollection<ShoppingCartActionLog> _logCollection;

        public ShoppingCartActionLogger(IOptions<MongoDbSettings> settings, IMongoClient client)
        {
            var db = client.GetDatabase(settings.Value.Database);
            _logCollection = db.GetCollection<ShoppingCartActionLog>(settings.Value.CollectionName);
        }

        public async Task LogAsync(ShoppingCartActionLog log)
        {
            try
            {
                await _logCollection.InsertOneAsync(log);
            }
            catch (Exception ex)
            {
                // Logging yapan logger’ı bozmayalım; konsol uyarısı yeterli
                Console.WriteLine($"[LOG ERROR] Mongo insert failed: {ex.Message}");
            }
        }
    }
}
