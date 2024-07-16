using MongoDB.Driver;
using SharedX.Core.Entities;
using SharedX.Core.Matching.DropCopy;

namespace TradeEngineX.Infra.Data;
public interface ITradeEngineContext
{
    MongoClient MongoClient { get; }
    IMongoCollection<TradeReport> TradeEngine { get; }
}
