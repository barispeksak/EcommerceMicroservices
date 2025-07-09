using MongoDB.Driver;
using ProductItemMicroservice_Data.Models;

namespace ProductItemMicroservice_Service.Services
{
    public class ProductItemActionLogger
    {
        private readonly IMongoCollection<ProductItemActionLog> _collection;

        public ProductItemActionLogger(IMongoDatabase db) =>
            _collection = db.GetCollection<ProductItemActionLog>("ProductItemActionLogs");

        public Task LogAsync(ProductItemActionLog log) =>
            _collection.InsertOneAsync(log);
    }
}