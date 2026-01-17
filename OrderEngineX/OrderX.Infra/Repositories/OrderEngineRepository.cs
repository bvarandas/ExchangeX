using FluentResults;
using Microsoft.Extensions.Logging;
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
    private static string ObjectId = "696befefc045d54c59745b78";
    public OrderEngineRepository(IOrderEngineContext context, ILogger<OrderEngineRepository> logger)
    {
        _logger = logger;
        _context = context;
    }
    private async Task<long> GetNextSequenceValueAsync(string sequenceName)
    {
        var filter = Builders<OrderIDEngine>.Filter.Eq(c => c.Id, sequenceName);
        var update = Builders<OrderIDEngine>.Update.Inc(c => c.OrderId, 1);
        var options = new FindOneAndUpdateOptions<OrderIDEngine>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var updatedCounter = await _context.OrderId.FindOneAndUpdateAsync(filter, update, options);
        return updatedCounter.OrderId;
    }

    public async Task<Result> CreateOrdersAsync(OrderEngine order, CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            order.OrderID = await this.GetNextSequenceValueAsync(ObjectId);

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

    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);


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