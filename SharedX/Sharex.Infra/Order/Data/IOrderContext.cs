using MongoDB.Driver;
namespace SharedX.Infra.Order.Data;
public interface IOrderContext
{
    IMongoCollection<Core.Matching.Order> OrderTrade { get; }
    IMongoCollection<long> OrderId { get; }
    MongoClient MongoClient { get; }
}
