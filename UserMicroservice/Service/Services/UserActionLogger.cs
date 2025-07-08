using MongoDB.Driver;
using UserMicroservice.Models;

namespace UserMicroservice.Service.Services
{
    public class UserActionLogger
    {
        private readonly IMongoCollection<UserActionLog> _logCollection;

        public UserActionLogger(IMongoDatabase database)
        {
            _logCollection = database.GetCollection<UserActionLog>("UserActionLogs");
        }

        public async Task LogAsync(UserActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
