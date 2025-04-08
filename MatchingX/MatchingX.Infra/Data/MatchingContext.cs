using MatchingX.Core.Entities;
using MongoDB.Driver;
namespace MatchingX.Infra.Data;
public class MatchingContext : IMatchingContext
{
    private const string MatchingCollectionName = "Matching";
    private readonly IMongoDatabase _database;
    public IMongoCollection<MatchOrder> _matching;
    public MongoClient MongoClient { get; }

    public MatchingContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient)database.Client;
    }

    public IMongoCollection<MatchOrder> Matching
    {
        get
        {
            if (_matching is null)
                _matching = _database.GetCollection<MatchOrder>(MatchingCollectionName);

            return _matching!;
        }
    }
}
