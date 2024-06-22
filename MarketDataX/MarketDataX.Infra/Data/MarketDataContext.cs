using MarketDataX.Core.Entities;
using MongoDB.Driver;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Infra.Data;
public class MarketDataContext : IMarketDataContext
{
    private const string MarketDataCollectionName = "MarketData";
    private const string LoginCollectionName = "Login";
    private const string MarketDataSnapshotCollectionName = "MarketDataSnapshot";
    
    private readonly IMongoDatabase _database;
    public MongoClient MongoClient { get; }

    public IMongoCollection<MarketData> _marketData ;
    public IMongoCollection<Login> _login;
    public IMongoCollection<MarketDataSnapshot> _marketDataSnapshot;

    public MarketDataContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient)database.Client;
    }
    public IMongoCollection<MarketDataSnapshot> MarketDataSnapshot
    {
        get
        {
            if (_marketDataSnapshot is null)
                _marketDataSnapshot = _database.GetCollection<MarketDataSnapshot>(MarketDataSnapshotCollectionName);

            return _marketDataSnapshot!;
        }
    }
    public IMongoCollection<Login> Login
    {
        get
        {
            if (_login is null)
                _login = _database.GetCollection<Login>(LoginCollectionName);

            return _login!;
        }
    }

    public IMongoCollection<MarketData> MarketData
    {
        get
        {
            if (_marketData is null)
                _marketData = _database.GetCollection<MarketData>(MarketDataCollectionName);

            return _marketData!;
        }
    }
}