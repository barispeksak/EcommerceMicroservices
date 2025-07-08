using MongoDB.Driver;
using ShippingTypeMicroservice.Models;

namespace ShippingTypeMicroservice.Service.Logging
{
    public class ShippingActionLogger
    {
        private readonly IMongoCollection<ShippingActionLog> _logCollection;

        public ShippingActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<ShippingActionLog>("ShippingActionLogs");
        }

        public async Task LogAsync(ShippingActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
    