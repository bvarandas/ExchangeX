using MongoDB.Driver;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Infra.Data;
public class MatchingContext : IMatchingContext
{
    private const string MatchingCollectionName = "Matching";
    private readonly IMongoDatabase _database;
    public IMongoCollection<OrderEngine> _matching;
    public MongoClient MongoClient { get; }

    public MatchingContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient)database.Client;
    }

    public IMongoCollection<OrderEngine> Matching
    {
        get
        {
            if (_matching is null)
                _matching = _database.GetCollection<OrderEngine>(MatchingCollectionName);

            return _matching!;
        }
    }
}
