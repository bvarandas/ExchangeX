
using MatchingX.Core.Entities;
using MongoDB.Driver;

namespace MatchingX.Infra.Data;

public interface IMatchingContext
{
    IMongoCollection<MatchOrder> Matching { get; }
    MongoClient MongoClient { get; }
}
