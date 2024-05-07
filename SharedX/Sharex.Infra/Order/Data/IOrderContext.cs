using MongoDB.Driver;
using SharedX.Core.Matching.OrderEngine;

namespace SharedX.Infra.Order.Data;
public interface IOrderContext
{
    IMongoCollection<OrderEngine> OrderTrade { get; }
    IMongoCollection<long> OrderId { get; }
    MongoClient MongoClient { get; }
}
