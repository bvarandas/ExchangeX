using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;

namespace MatchingX.Infra.Data;
public class ExecutedTradeContext : IExecutedTradeContext
{
    public IMongoCollection<TradeCaptureReport> ExecutedTrade { get; }
    public ExecutedTradeContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        ExecutedTrade = database.GetCollection<TradeCaptureReport>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameTrade"));
    }
}