using MatchingX.Core.Interfaces;
using MatchingX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Enums;
using MongoDB.Bson;
using FluentResults;
namespace MatchingX.Infra.Repositories;
public class MatchingRepository : IMatchingRepository
{
    private readonly IMatchingContext _context;
    private readonly ILogger<MatchingRepository> _logger;
    public MatchingRepository(IMatchingContext context, ILogger<MatchingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> UpsertOrderMatchingAsync(OrderEngine orderEngine, CancellationToken cancellation)
    {
        bool result = false;
        var clientSessionOptions = new ClientSessionOptions();
        using (var session = await _context.MongoClient.StartSessionAsync(clientSessionOptions, cancellation))
        {
            session.StartTransaction();
            try
            {
                if ( string.IsNullOrEmpty( orderEngine.Id) )
                    orderEngine.Id =  ObjectId.GenerateNewId().ToString();

                var builder = Builders<OrderEngine>.Filter;
                var filter = builder.Eq(o=>o.OrderID, orderEngine.OrderID);

                var resultReplace = await _context.Matching.ReplaceOneAsync(session,
                filter,
                replacement: orderEngine,
                options: new ReplaceOptions { IsUpsert = true },
                cancellation);

                result = resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0;

                await session.CommitTransactionAsync();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));
            }
        }
        return Result.Ok();
    } 

    public async Task<Result> RemoveOrdersMatchingAsync(List<long> IdOrders, CancellationToken cancellation)
    {
        bool result = false;
        var clientSessionOptions = new ClientSessionOptions();
        using (var session = await _context.MongoClient.StartSessionAsync(clientSessionOptions, cancellation))
        {
            session.StartTransaction();
            try
            {
                var filterDelete = Builders<OrderEngine>.Filter
                            .In(o => o.OrderID, IdOrders);

                var resultDelete = await _context.Matching.DeleteManyAsync(session, filterDelete);
                result = resultDelete.IsAcknowledged && resultDelete.DeletedCount > 0;

                await session.CommitTransactionAsync();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await session.AbortTransactionAsync();
                return Result.Fail(new Error(ex.Message));
            }
        }
        return Result.Ok();
    }

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> MatchingLimitAsync(OrderEngine orderEngine,  CancellationToken cancellation)
    {
        var result = default((OrderStatus, Dictionary<long, OrderEngine>))!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.Symbol, orderEngine.Symbol);
            var sort = Builders<OrderEngine>.Sort.Ascending("Price");

            var orderToExecute = new Dictionary<long, OrderEngine>();
            var clientSessionOptions = new ClientSessionOptions();
            
            using (var session = await _context.MongoClient.StartSessionAsync(clientSessionOptions, cancellation))
            {
                session.StartTransaction();
                
                try
                {

                    if (orderEngine.Side == SideTrade.Buy)
                    {
                        filter = filter & builder.Eq(o => o.Side, SideTrade.Sell);
                        sort = Builders<OrderEngine>.Sort.Descending("Price");
                    }
                    else if (orderEngine.Side == SideTrade.Sell)
                    {
                        filter = filter & builder.Eq(o => o.Side, SideTrade.Buy);
                        sort = Builders<OrderEngine>.Sort.Ascending("Price");
                    }

                    if (orderEngine.TimeInForce != TimeInForce.FOK)
                    {
                        if (orderEngine.Side == SideTrade.Buy)
                            filter = filter & builder.Lte(o => o.Price, orderEngine.Price);
                        else if (orderEngine.Side == SideTrade.Sell)
                            filter = filter & builder.Gte(o => o.Price, orderEngine.Price);

                        filter = filter & builder.Gte(o=>o.LeavesQuantity ,orderEngine.Quantity);

                    }else if(orderEngine.TimeInForce == TimeInForce.FOK)
                    {
                        if (orderEngine.Side == SideTrade.Buy)
                            filter = filter & builder.Lte(o => o.Price, orderEngine.Price);
                        else if (orderEngine.Side == SideTrade.Sell)
                            filter = filter & builder.Gte(o => o.Price, orderEngine.Price);

                        filter = filter & builder.Gte(o => o.LeavesQuantity, orderEngine.Quantity);
                    }

                    var orders = await _context.Matching.FindAsync(session, filter,
                        new FindOptions<OrderEngine, OrderEngine>
                        {
                            Sort = sort,
                        });

                    var ordersToRemove = new List<long>();

                    if (!await orders.AnyAsync() && orderEngine.TimeInForce == TimeInForce.FOK)
                    {
                        result.Item1 = OrderStatus.Cancelled;
                        ordersToRemove.Add(orderEngine.OrderID);
                    }

                    DateTime now = DateTime.Now;
                    decimal quantityCollected = 0.0M;
                    await orders.ForEachAsync(order => 
                    {
                        if (orderEngine.Quantity > quantityCollected && (quantityCollected + order.LeavesQuantity) < orderEngine.Quantity)
                        {
                            quantityCollected += order.LeavesQuantity;
                            orderEngine.LeavesQuantity -= order.LeavesQuantity;
                            order.LeavesQuantity = 0;
                            order.OrderStatus = OrderStatus.Filled;
                            order.TransactTime = now;

                            orderEngine.LastPrice = order.Price;
                            order.LastPrice = order.Price;

                            ordersToRemove.Add(order.OrderID);
                            orderToExecute.Add(order.OrderID, order);
                        }
                    });
                                                            
                    if (quantityCollected > 0 && quantityCollected != orderEngine.Quantity)
                    {
                        result.Item1 = orderEngine.OrderStatus =OrderStatus.PartiallyFilled;
                        result.Item2 = orderToExecute;

                        ordersToRemove.Add(orderEngine.OrderID);
                    }
                    else if(quantityCollected > 0 && quantityCollected == orderEngine.Quantity)
                    {
                        result.Item1 = orderEngine.OrderStatus = OrderStatus.Filled;
                        result.Item2 = orderToExecute;

                        ordersToRemove.Add(orderEngine.OrderID);
                    }

                    if (ordersToRemove.Count > 0)
                    {
                        var filterDelete = Builders<OrderEngine>.Filter
                            .In(o => o.OrderID, ordersToRemove);

                        var resultDelete = await _context.Matching.DeleteManyAsync(session, filterDelete);

                        if (resultDelete.IsAcknowledged && resultDelete.DeletedCount > 0)
                        {
                            _logger.LogInformation("Orders deleted with success!");
                        }
                    }

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

    public async Task<(OrderStatus, Dictionary<long, OrderEngine>)> MatchingMarketAsync(OrderEngine orderEngine, CancellationToken cancellation)
    {
        var result = default((OrderStatus, Dictionary<long, OrderEngine>))!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.Symbol, orderEngine.Symbol);
            var sort = Builders<OrderEngine>.Sort.Ascending("Price");

            var orderToExecute = new Dictionary<long, OrderEngine>();
            var clientSessionOptions = new ClientSessionOptions();
            
            using (var session = await _context.MongoClient.StartSessionAsync(clientSessionOptions, cancellation))
            {
                var transactionOptions = new MongoDB.Driver.TransactionOptions();
                session.StartTransaction(transactionOptions);
                try
                {
                    if (orderEngine.Side == SideTrade.Buy)
                    {
                        filter = filter & builder.Eq(o => o.Side, SideTrade.Sell);
                        sort = Builders<OrderEngine>.Sort.Descending("Price");
                    }
                    else if (orderEngine.Side == SideTrade.Sell)
                    {
                        filter = filter & builder.Eq(o => o.Side, SideTrade.Buy);
                        sort = Builders<OrderEngine>.Sort.Ascending("Price");
                    }
                    
                    filter = filter & builder.Gte(o => o.LeavesQuantity, orderEngine.LeavesQuantity);
                    
                    var orders = await _context.Matching.FindAsync(session, filter,
                        new FindOptions<OrderEngine, OrderEngine>
                        {
                            Sort = sort,
                        });

                    var ordersToRemove = new List<long>();

                    if (!await orders.AnyAsync() && orderEngine.TimeInForce == TimeInForce.FOK)
                    {
                        result.Item1 = OrderStatus.Cancelled;
                        ordersToRemove.Add(orderEngine.OrderID);
                    }
                    DateTime now = DateTime.Now;
                    decimal quantityCollected = 0.0M;
                    await orders.ForEachAsync(order => {

                        if (orderEngine.Quantity > quantityCollected && (quantityCollected + order.LeavesQuantity) < orderEngine.Quantity)
                        {
                            quantityCollected += order.LeavesQuantity;
                            orderEngine.LeavesQuantity -= order.LeavesQuantity;
                            order.LeavesQuantity = 0;
                            order.OrderStatus = OrderStatus.Filled;
                            order.TransactTime = now;


                            orderEngine.LastPrice = order.Price;
                            order.LastPrice = order.Price;

                            ordersToRemove.Add(order.OrderID);
                            orderToExecute.Add(order.OrderID, order);
                        }
                    });

                    if (quantityCollected > 0 && quantityCollected != orderEngine.LeavesQuantity)
                    {
                        result.Item1 = OrderStatus.PartiallyFilled;
                        result.Item2 = orderToExecute;

                        ordersToRemove.Add(orderEngine.OrderID);
                    }
                    else if (quantityCollected > 0 && quantityCollected == orderEngine.LeavesQuantity)
                    {
                        result.Item1 = OrderStatus.Filled;
                        result.Item2 = orderToExecute;

                        ordersToRemove.Add(orderEngine.OrderID);
                    }

                    if (ordersToRemove.Count > 0)
                    {
                        var filterDelete = Builders<OrderEngine>.Filter
                            .In(o => o.OrderID, ordersToRemove);

                        var resultDelete = await _context.Matching.DeleteManyAsync(session, filterDelete);

                        if (resultDelete.IsAcknowledged && resultDelete.DeletedCount > 0)
                        {
                            _logger.LogInformation("Orders deleted with success!");
                        }
                    }

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

}
