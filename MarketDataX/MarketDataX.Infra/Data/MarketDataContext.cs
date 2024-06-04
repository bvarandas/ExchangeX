using MarketDataX.Core.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Infra.Data;
public class MarketDataContext : IMarketDataContext
{
    public IMongoCollection<MarketData> MarketData { get; }
    public IMongoCollection<Login> Login { get; }
    public IMongoCollection<MarketDataSnapshot> MarketDataSnapshot { get; }

    public MarketDataContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        Login = database.GetCollection<Login>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameLogin"));

        MarketData = database.GetCollection<MarketData>(
            configuration.GetValue<string>("DatabaseSettings:CollectionMarketData"));

        MarketDataSnapshot = database.GetCollection<MarketDataSnapshot>(
            configuration.GetValue<string>("DatabaseSettings:CollectionMarketDataSnapshot"));
    }
}
