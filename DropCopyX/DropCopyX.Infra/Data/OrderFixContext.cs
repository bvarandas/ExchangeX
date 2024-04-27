using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching;
namespace DropCopyX.Infra.Data;
public class OrderFixContext : IOrderFixContext
{
    public IMongoCollection<Order> OrderTrade { get; }
    public OrderFixContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        OrderTrade = database.GetCollection<Order>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameOrder"));
    }
}