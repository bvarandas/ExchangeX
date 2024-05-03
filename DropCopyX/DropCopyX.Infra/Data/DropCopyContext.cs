using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Matching;
using SharedX.Core.Matching.DropCopy;

namespace DropCopyX.Infra.Data;
public class DropCopyContext : IDropCopyContext
{
    public IMongoCollection<ExecutionReport> ExecutionReport { get; }
    public DropCopyContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
        ExecutionReport = database.GetCollection<ExecutionReport>(
            configuration.GetValue<string>("DatabaseSettings:CollectionExecutionReport"));
    }
}
