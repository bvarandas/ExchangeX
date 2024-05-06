using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Infra.Data;
public class OrderEngineContext :  IOrderEngineContext
{
    public IMongoCollection<OrderEngine> OrderEngine { get; }
    public OrderEngineContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        OrderEngine = database.GetCollection<OrderEngine>(
            configuration.GetValue<string>("DatabaseSettings:CollectionOrderEntry"));
    }
}