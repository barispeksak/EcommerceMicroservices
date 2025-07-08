using MongoDB.Driver;
using VariationOptionMicroservice.Models;

namespace VariationOptionMicroservice.Service.Services
{
    public class VariationOptionActionLogger
    {
        private readonly IMongoCollection<VariationOptionActionLog> _logCollection;

        public VariationOptionActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<VariationOptionActionLog>("VariationOptionActionLogs");
        }

        public async Task LogAsync(VariationOptionActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }   
    }
}
