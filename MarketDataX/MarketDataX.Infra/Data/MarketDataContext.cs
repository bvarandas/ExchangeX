using MarketDataX.Core.Entities;
using MarketDataX.Infra.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching.MarketData;
namespace DropCopyX.Infra.Data;
public class MarketDataContext : IMarketDataContext
{
    public IMongoCollection<MarketData> MarketData { get; }
    public IMongoCollection<Login> Login { get; }

    public MarketDataContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        Login = database.GetCollection<Login>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameLogin"));

        MarketData = database.GetCollection<MarketData>(
            configuration.GetValue<string>("DatabaseSettings:CollectionIncremental"));
    }
}
