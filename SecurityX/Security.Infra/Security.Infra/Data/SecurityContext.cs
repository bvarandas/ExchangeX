using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SharedX.Core.Entities;
namespace Security.Infra.Data;
public class SecurityContext : ISecurityContext
{
    private readonly IMongoDatabase _database;
    private IMongoCollection<SecurityEngine> _securityEngine;
    private const string SecurityCollectionName = "SecurityEngine";
    public MongoClient MongoClient { get; }
    //public SecurityContext(IConfiguration configuration)
    //{
    //    MongoClient = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
    //    var database = MongoClient.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
    //    SecurityEngine = database.GetCollection<SecurityEngine>(
    //        configuration.GetValue<string>("DatabaseSettings:CollectionNameSecurity"));
    //}

    public SecurityContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient) database.Client;
    }

    public IMongoCollection<SecurityEngine> SecurityEngine
    {
        get
        {
            if (_securityEngine is null)
                _securityEngine = _database.GetCollection<SecurityEngine>(SecurityCollectionName);

            return _securityEngine;
        }
    }
}