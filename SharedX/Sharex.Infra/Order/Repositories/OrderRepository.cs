using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using QuickFix.Fields;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Infra.Order.Data;
namespace SharedX.Infra.Order.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly IOrderContext _context;
    private readonly ILogger<OrderRepository> _logger;
    public OrderRepository(IOrderContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<bool> CreateOrdersAsync(OrderEngine  order, CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            var inserts = new List<WriteModel<OrderEngine>>();
            inserts.Add(new InsertOneModel<OrderEngine>(order));

            var insertResult = await _context.OrderTrade.BulkWriteAsync(inserts, null, cancellation);
            result = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }
    public async Task<long> GetOrderIdAsync(CancellationToken cancellation)
    {
        long result = 0;
        try
        {
            var builder = Builders<OrderIDEngine>.Filter;
            var filter = builder.Empty;

            using (var session = await _context.MongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {

                    var single = await _context.OrderId.Find(session, filter)
                        .FirstOrDefaultAsync(cancellation);

                    single = single ?? new OrderIDEngine();
                    
                    if ( string.IsNullOrEmpty( single.Id))
                        single.Id = ObjectId.GenerateNewId().ToString();

                    single.OrderId++;
                    result = single.OrderId;


                    var resultReplace = await _context.OrderId.ReplaceOneAsync(session,
                        filter,
                    replacement: new OrderIDEngine() { OrderId = result , Id = single.Id},
                    options: new ReplaceOptions { IsUpsert = true },
                    cancellation);

                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    await session.AbortTransactionAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }

    public async Task<IEnumerable<OrderEngine>> GetOrdersByAccountIdAsync(int accountId, CancellationToken cancellation)
    {
        IEnumerable<OrderEngine> result = null!;

        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.AccountId, accountId);

            result = _context.OrderTrade.Find(filter).ToEnumerable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        return result;
    }

    public async Task<OrderEngine> GetOrderByIdAsync(long orderId, CancellationToken cancellation)
    {
        OrderEngine result = null!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.OrderID, orderId);
            result = await _context.OrderTrade.Find(filter).SingleAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        return result;
    }

    public async Task<IEnumerable<OrderEngine>> GetOrdersOnRestartAsync(CancellationToken cancellation)
    {
        IEnumerable<OrderEngine> result = null!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.OrderStatus, OrderStatus.New)| 
                         builder.Eq(o=>o.OrderStatus, OrderStatus.PartiallyFilled);
            
            var orders = await _context.OrderTrade.FindAsync(filter);
            result = orders.ToEnumerable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }
    public async Task<bool> UpdateOrderDetailAsync(OrderEngine order, OrderEngineDetail oderDetail, CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Where(x => x.OrderID == order.OrderID);

            var orderSingle = await _context.OrderTrade.Find(filter).SingleAsync(cancellation);

            if (orderSingle.OrderDetails is null)
                orderSingle.OrderDetails = new List<OrderEngineDetail>();

            orderSingle.OrderDetails.Add(oderDetail);

            var resultReplace = await _context.OrderTrade.ReplaceOneAsync(
                null,
            replacement: orderSingle,
            options: new ReplaceOptions { IsUpsert = true },
            cancellation);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }

    public async Task<bool> UpdateOrderAsync(OrderEngine order,  CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            var resultReplace = await _context.OrderTrade.ReplaceOneAsync(
                null,
            replacement: order,
            options: new ReplaceOptions { IsUpsert = true },
            cancellation);

            result = resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }
}