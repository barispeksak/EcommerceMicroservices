using MongoDB.Driver;
using ProductCategoryMicroservice_Data.Models;

namespace ProductCategoryMicroservice_Service.Services
{
    public class ProductCategoryActionLogger
    {
        private readonly IMongoCollection<ProductCategoryActionLog> _collection;

        public ProductCategoryActionLogger(IMongoDatabase db) =>
            _collection = db.GetCollection<ProductCategoryActionLog>("ProductCategoryActionLogs");

        public Task LogAsync(ProductCategoryActionLog log) =>
            _collection.InsertOneAsync(log);
    }
}