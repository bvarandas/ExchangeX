using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;
namespace DropCopyX.Infra.Data;
public class DropCopyContext : IDropCopyContext
{
    private readonly IMongoDatabase _database;
    
    private const string DropCopyCollectionName = "DropCopy";
    public MongoClient MongoClient { get; }

    public IMongoCollection<ExecutionReport> _executionReport;
    public DropCopyContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient)database.Client;
    }

    public IMongoCollection<ExecutionReport> ExecutionReport 
    {
        get
        {
            if (_executionReport is null)
                _executionReport = _database.GetCollection<ExecutionReport>(DropCopyCollectionName);

            return _executionReport;
        }
    }
}