using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using MongoDB.Driver;
using ShoppingCartMicroservice_Service.Models; 

namespace ShoppingCartMicroservice_Service.Services
{
    public class ShoppingCartActionLogger
    {
        private readonly IMongoCollection<ShoppingCartActionLog> _logCollection;

        public ShoppingCartActionLogger(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDb:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDb:Database"]);
            _logCollection = database.GetCollection<ShoppingCartActionLog>("ShoppingCartLogs");
        }

        public async Task LogAsync(ShoppingCartActionLog log)
        {
            await _logCollection.InsertOneAsync(log);
        }
    }
}
