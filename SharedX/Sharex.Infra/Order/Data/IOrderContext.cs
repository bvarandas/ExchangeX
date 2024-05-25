using MongoDB.Driver;
using SharedX.Core.Entities;
using SharedX.Core.Matching.OrderEngine;
namespace SharedX.Infra.Order.Data;
public interface IOrderContext
{
    IMongoCollection<OrderEngine> OrderTrade { get; }
    IMongoCollection<OrderIDEngine> OrderId { get; }
    MongoClient MongoClient { get; }
}
