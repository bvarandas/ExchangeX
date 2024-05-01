using DropCopyX.Core.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching;
using SharedX.Core.Matching.DropCopy;

namespace DropCopyX.Infra.Data;
public class DropCopyContext : IDropCopyContext
{
    public IMongoCollection<ExecutionReport> ExecutionReport { get; }
    public IMongoCollection<Login> Login { get; }

    public DropCopyContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

        Login = database.GetCollection<Login>(
            configuration.GetValue<string>("DatabaseSettings:CollectionNameLogin"));

        ExecutionReport = database.GetCollection<ExecutionReport>(
            configuration.GetValue<string>("DatabaseSettings:CollectionExecutionReport"));
    }
}
