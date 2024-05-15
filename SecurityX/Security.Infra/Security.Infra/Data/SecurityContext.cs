using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Entities;
namespace Security.Infra.Data;
public class SecurityContext : ISecurityContext
{
    public IMongoCollection<SecurityEngine> SecurityEngine { get; }
    public MongoClient MongoClient { get; }
    public SecurityContext(IConfiguration configuration)
    {
        MongoClient = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = MongoClient.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
        SecurityEngine = database.GetCollection<SecurityEngine>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameSecurity"));
    }
}