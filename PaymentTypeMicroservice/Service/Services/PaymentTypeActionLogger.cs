using MongoDB.Driver;
using PaymentTypeMicroservice.Models;

namespace PaymentTypeMicroservice.Services.Logging
{
    public class PaymentTypeActionLogger
    {
        private readonly IMongoCollection<PaymentTypeActionLog> _logCollection;

        public PaymentTypeActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<PaymentTypeActionLog>("PaymentTypeActionLogs");
        }

        public async Task LogAsync(PaymentTypeActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
    