using MongoDB.Driver;
using SharedX.Core.Matching;
namespace MatchingX.Infra.Data;
public interface ITradeContext
{
    IMongoCollection<Trade> Trade { get; }
    MongoClient MongoClient { get; }
}