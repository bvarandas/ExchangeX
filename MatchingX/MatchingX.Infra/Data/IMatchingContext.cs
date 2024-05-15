
using MongoDB.Driver;
using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;

namespace MatchingX.Infra.Data;

public interface IMatchingContext
{
    IMongoCollection<OrderEngine> Matching { get; }
    MongoClient MongoClient { get; }
}
