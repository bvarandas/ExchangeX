using MongoDB.Driver;
using SharedX.Core.Entities;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Infra.Data;
public class OrderEngineContext :  IOrderEngineContext
{
    private readonly IMongoDatabase _database;
    private const string OrderEngineCollectionName = "OrderEngine";
    private const string OrderIdCollectionName = "OrderId";

    public MongoClient MongoClient { get; }
    private IMongoCollection<OrderEngine> _orderEngine;
    private IMongoCollection<OrderIDEngine> _orderId;


    public OrderEngineContext(IMongoDatabase database)
    {
        this._database = database;
        MongoClient = (MongoClient)database.Client;
    }

    public IMongoCollection<OrderEngine> OrderEngine
    {
        get
        {
            if (_orderEngine is null)
                _orderEngine = _database.GetCollection<OrderEngine>(OrderEngineCollectionName);

            return _orderEngine!;
        }
    }

    public IMongoCollection<OrderIDEngine> OrderId
    {
        get
        {
            if (_orderId is null)
                _orderId = _database.GetCollection<OrderIDEngine>(OrderIdCollectionName);

            return _orderId!;
        }
    }
}