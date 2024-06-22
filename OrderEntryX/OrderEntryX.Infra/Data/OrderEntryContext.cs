using MongoDB.Driver;
using SharedX.Core.Entities;
namespace OrderEntryX.Infra.Data;
public class OrderEntryContext :  IOrderEntryContext
{
    private readonly IMongoDatabase _database;
    private const string OrderEntryCollectionName = "OrderEntry";
    public MongoClient MongoClient { get; }
    public IMongoCollection<Login> _login;
    public OrderEntryContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient)database.Client;
    }
    public IMongoCollection<Login> Login
    {
        get
        {
            if (_login is null)
                _login = _database.GetCollection<Login>(OrderEntryCollectionName);
            
            return _login!;
        }
    }
}