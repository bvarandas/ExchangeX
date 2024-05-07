using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching.OrderEngine;

namespace SharedX.Infra.Order.Data;
public class OrderContext : IOrderContext
{
    public IMongoCollection<OrderEngine> OrderTrade { get; }
    public MongoClient MongoClient { get; }

    public IMongoCollection<long> OrderId { get; }

    public OrderContext(IConfiguration configuration)
    {
        MongoClient = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = MongoClient.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        OrderTrade = database.GetCollection<OrderEngine>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameOrder"));

        OrderId = database.GetCollection<long>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameOrderId"));
    }
}