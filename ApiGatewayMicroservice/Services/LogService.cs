// Services/LogService.cs
using MongoDB.Driver;
using MongoDB.Bson;
using ApiGateway.Models;

namespace ApiGateway.Services
{
    public class LogService
    {
        private readonly IMongoCollection<RequestLog> _logCollection;

        public LogService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDb:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDb:Database"]);
            _logCollection = database.GetCollection<RequestLog>("RequestLogs");
        }

        public async Task InsertLogAsync(RequestLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
