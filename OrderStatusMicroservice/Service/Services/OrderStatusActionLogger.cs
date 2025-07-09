using MongoDB.Driver;
using OrderStatusMicroservice.Models;

namespace OrderStatusMicroservice.Services.Logging
{
    public class OrderStatusActionLogger
    {
        private readonly IMongoCollection<OrderStatusActionLog> _logCollection;

        public OrderStatusActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<OrderStatusActionLog>("OrderStatusActionLogs");
        }

        public async Task LogAsync(OrderStatusActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
    