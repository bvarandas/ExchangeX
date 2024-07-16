using MongoDB.Driver;
using SharedX.Core.Entities;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Infra.Data;
public interface IOrderEngineContext
{
    IMongoCollection<OrderEngine> OrderEngine { get; }
    IMongoCollection<OrderIDEngine> OrderId { get; }
    MongoClient MongoClient { get; }
}