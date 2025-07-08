using MongoDB.Driver;
using VariationMicroservice.Models;

namespace VariationMicroservice.Service.Services
{
    public class VariationActionLogger
    {
        private readonly IMongoCollection<VariationActionLog> _logCollection;

        public VariationActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<VariationActionLog>("VariationActionLogs");
        }

        public async Task LogAsync(VariationActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }   
    }
}
