using MongoDB.Driver;
using ProductConfigurationMicroservice_Data.Models;

namespace ProductConfigurationMicroservice_Service.Services
{
    public class ProductConfigurationActionLogger
    {
        private readonly IMongoCollection<ProductConfigurationActionLog> _collection;

        public ProductConfigurationActionLogger(IMongoDatabase db) =>
            _collection = db.GetCollection<ProductConfigurationActionLog>("ProductConfigurationActionLogs");

        public Task LogAsync(ProductConfigurationActionLog log) =>
            _collection.InsertOneAsync(log);
    }
}