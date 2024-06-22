using MongoDB.Driver;
using SharedX.Core.Matching;
namespace MatchingX.Infra.Data;
public class TradeContext : ITradeContext
{
    private const string TradeCollectionName = "Trade";
    private readonly IMongoDatabase _database;
    public IMongoCollection<Trade> _trade;
    public MongoClient MongoClient { get; }
    public TradeContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient)database.Client;
    }

    public IMongoCollection<Trade> Trade
    {
        get
        {
            if (_trade is null)
                _trade = _database.GetCollection<Trade>(TradeCollectionName);

            return _trade!;
        }
    }
}