using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using OrderEntryX.Core.Entities;
using SharedX.Core.Entities;
namespace OrderEntryX.Infra.Data;
public class OrderEntryContext :  IOrderEntryContext
{
    public IMongoCollection<Login> Login { get; }

    public OrderEntryContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        Login = database.GetCollection<Login>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameLogin"));
    }
}