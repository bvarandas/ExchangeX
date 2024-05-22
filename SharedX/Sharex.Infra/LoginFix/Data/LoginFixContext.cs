using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Entities;

namespace Sharedx.Infra.LoginFix.Data;
public class LoginFixContext : ILoginFixContext
{
    public IMongoCollection<Login> Login { get; }

    public LoginFixContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        Login = database.GetCollection<Login>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameLogin"));
    }
}