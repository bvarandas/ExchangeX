using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
            var builder = Builders<long>.Filter;
            var filter = builder.Empty;

            using (var session = await _context.MongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var single = await _context.OrderId.Find(session, filter).SingleAsync(cancellation);
                    single++;

                    var resultReplace = await _context.OrderId.ReplaceOneAsync(session,
                        null,
                    replacement: single,
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
    public async Task<IEnumerable<OrderEngine>> GetOrdersOnRestartAsync(CancellationToken cancellation)
    {
        IEnumerable<OrderEngine> result = null!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.OrderStatus, OrderStatus.New);
            var orders = await _context.OrderTrade.FindAsync(filter);

            result = orders.ToEnumerable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }
    public async Task<bool> UpdateOrderAsync(OrderEngine order, CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Where(x => x.OrderID == order.OrderID);

            var orderSingle = await _context.OrderTrade.Find(filter).SingleAsync(cancellation);

            if (orderSingle.OrderDetails is null)
                orderSingle.OrderDetails = new List<OrderEngineDetail>();

            orderSingle.OrderDetails.Add((OrderEngineDetail)order);

            var resultReplace = await _context.OrderTrade.ReplaceOneAsync(
                null,
            replacement: orderSingle,
            options: new ReplaceOptions { IsUpsert = true },
            cancellation);


            //var updates = new List<WriteModel<OrderEngine>>();
            //var filterBuilder = Builders<OrderEngine>.Filter;

            //var filter = filterBuilder.Where(x => x.OrderID == order.OrderID);
            //updates.Add(new ReplaceOneModel< OrderEngine>(filter, order));

            //var updateResult = await _context.OrderTrade.BulkWriteAsync(updates, null, cancellation);
            //result = updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
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