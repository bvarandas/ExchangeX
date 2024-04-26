using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching;
namespace MatchingX.Infra.Data;
public class OrderContext : IOrderContext
{
    public IMongoCollection<Order> OrderTrade { get; }
    public OrderContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        OrderTrade = database.GetCollection<Order>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameOrder"));
    }
}