using MongoDB.Driver;
using UserAddressMicroservice.Models;

namespace UserAddressMicroservice.Service.Logging
{
    public class UserAddressActionLogger
    {
        private readonly IMongoCollection<UserAddressActionLog> _logCollection;

        public UserAddressActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<UserAddressActionLog>("UserAddressActionLogs");
        }

        public async Task LogAsync(UserAddressActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
    