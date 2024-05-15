using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching;
namespace MatchingX.Infra.Data;
public class TradeContext : ITradeContext
{
    public IMongoCollection<Trade> Trade {  get;  }
    public MongoClient MongoClient { get; }
    public TradeContext(IConfiguration configuration)
    {
        MongoClient = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = MongoClient.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        Trade = database.GetCollection<Trade>(
            configuration.GetValue<string>("DatabaseSettings:CollectionTrade"));
    }
}