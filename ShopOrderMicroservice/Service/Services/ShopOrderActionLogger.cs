using MongoDB.Driver;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Services.Logging
{
    public class ShopOrderActionLogger
    {
        private readonly IMongoCollection<ShopOrderActionLog> _logCollection;

        public ShopOrderActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<ShopOrderActionLog>("ShopOrderActionLogs");
        }

        public async Task LogAsync(ShopOrderActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
    