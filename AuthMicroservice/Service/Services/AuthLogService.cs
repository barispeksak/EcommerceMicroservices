using MongoDB.Driver;
using System.Threading.Tasks;

public class AuthLogService
{
    private readonly IMongoCollection<AuthLog> _logCollection;

    public AuthLogService(IMongoDatabase mongoDatabase)
    {
        _logCollection = mongoDatabase.GetCollection<AuthLog>("AuthLogs");
    }

    public async Task LogAsync(AuthLog log)
    {
        await _logCollection.InsertOneAsync(log);
    }
}
