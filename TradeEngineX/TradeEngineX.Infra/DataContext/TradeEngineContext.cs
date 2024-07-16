using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;

namespace TradeEngineX.Infra.Data;
public class TradeEngineContext : ITradeEngineContext
{
    private readonly IMongoDatabase _database;
    private IMongoCollection<TradeReport> _tradeEngine;
    private const string TradeEngineCollectionName = "TradeEngine";
    public MongoClient MongoClient { get; }

    public TradeEngineContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = database.Client! as MongoClient;
    }

    public IMongoCollection<TradeReport> TradeEngine
    {
        get
        {
            if (_tradeEngine is null)
                _tradeEngine = _database.GetCollection<TradeReport>(TradeEngineCollectionName);

            return _tradeEngine;
        }
    }
}