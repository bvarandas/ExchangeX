using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using OrderEngineX.Core.Interfaces;
using OrderEngineX.Infra.Data;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;

namespace OrderEngineX.Infra.Repositories;
public class OrderEngineRepository : IOrderEngineRepository
{
    private readonly IOrderEngineContext _context;
    private readonly ILogger<OrderEngineRepository> _logger;
    public OrderEngineRepository(IOrderEngineContext context, ILogger<OrderEngineRepository> logger)
    {
        _logger = logger;
        _context = context;
    }
    public async Task<Result> CreateOrdersAsync(OrderEngine order, CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            var inserts = new List<WriteModel<OrderEngine>>();
            inserts.Add(new InsertOneModel<OrderEngine>(order));

            var insertResult = await _context.OrderEngine.BulkWriteAsync(inserts, null, cancellation);
            result = insertResult.IsAcknowledged && insertResult.InsertedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }
    public async Task<Result> UpsertOrdersAsync(OrderEngine order, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.OrderID, order.OrderID);
            //var single = await _context.OrderEngine.Find( filter).SingleAsync(cancellationToken);

            var resultReplace = await _context.OrderEngine.ReplaceOneAsync(filter,
                replacement: order,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken);

            result = resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }

    public async Task<Result<long>> GetOrderIdAsync(CancellationToken cancellation)
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

                    if (string.IsNullOrEmpty(single.Id))
                        single.Id = ObjectId.GenerateNewId().ToString();

                    single.OrderId++;
                    result = single.OrderId;

                    filter = Builders<OrderIDEngine>.Filter.Eq(r => r.Id, single.Id);

                    var update = Builders<OrderIDEngine>.Update.Set(r => r.OrderId, result);


                    var resultReplace = await _context.OrderId.UpdateOneAsync(session, filter, update);
                    //    filter,
                    //update: new OrderIDEngine() { OrderId = result, Id = single.Id },
                    //options: new ReplaceOptions { IsUpsert = true },
                    //cancellation);

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
    public async Task<Result<IEnumerable<OrderEngine>>> GetOrdersByAccountIdAsync(int accountId, CancellationToken cancellation)
    {
        IEnumerable<OrderEngine> result = null!;

        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.AccountId, accountId);

            result = _context.OrderEngine.Find(filter).ToEnumerable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }

        return Result.Ok(result);
    }
    public async Task<Result<OrderEngine>> GetOrderByIdAsync(long orderId, CancellationToken cancellation)
    {
        OrderEngine result = null!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.OrderID, orderId);
            result = await _context.OrderEngine.Find(filter).SingleAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }

        return Result.Ok(result);
    }
    public async Task<Result<IEnumerable<OrderEngine>>> GetOrdersOnRestartAsync(CancellationToken cancellation)
    {
        IEnumerable<OrderEngine> result = null!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.OrderStatus, OrderStatus.New) |
                         builder.Eq(o => o.OrderStatus, OrderStatus.PartiallyFilled);

            var orders = await _context.OrderEngine.FindAsync(filter);
            result = orders.ToEnumerable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok(result);
    }
    public async Task<Result> UpdateOrderDetailAsync(OrderEngine order, OrderEngineDetail oderDetail, CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Where(x => x.OrderID == order.OrderID);

            var orderSingle = await _context.OrderEngine.Find(filter).SingleAsync(cancellation);

            if (orderSingle.OrderDetails is null)
                orderSingle.OrderDetails = new List<OrderEngineDetail>();

            orderSingle.OrderDetails.Add(oderDetail);

            var resultReplace = await _context.OrderEngine.ReplaceOneAsync(
                null,
            replacement: orderSingle,
            options: new ReplaceOptions { IsUpsert = true },
            cancellation);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }
}