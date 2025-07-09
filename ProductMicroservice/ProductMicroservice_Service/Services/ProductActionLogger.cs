using MongoDB.Driver;
using ProductMicroservice_Data.Models;

namespace ProductMicroservice_Service.Services
{
    public class ProductActionLogger
    {
        private readonly IMongoCollection<ProductActionLog> _collection;

        public ProductActionLogger(IMongoDatabase db) =>
            _collection = db.GetCollection<ProductActionLog>("ProductActionLogs");

        public Task LogAsync(ProductActionLog log) =>
            _collection.InsertOneAsync(log);
    }
}