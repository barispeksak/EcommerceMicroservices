using MongoDB.Driver;
using AddressMicroservice.Models;

namespace AddressMicroservice.Service.Services
{
    public class AddressActionLogger
    {
        private readonly IMongoCollection<AddressActionLog> _logCollection;

        public AddressActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<AddressActionLog>("AddressActionLogs");
        }

        public async Task LogAsync(AddressActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
