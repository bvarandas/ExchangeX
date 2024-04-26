using FluentResults;
using MatchingX.Core.Filters;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedX.Core.Matching;
namespace MatchingX.Infra.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly IOrderContext _context;
    private readonly ILogger<OrderRepository> _logger;
    public OrderRepository(IOrderContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<bool> CreateOrdersAsync(Order order, CancellationToken cancellation)
    {
        bool result = false;
        try
        { 
            var inserts = new List<WriteModel<Order>>();

            inserts.Add(new InsertOneModel<Order>(order));

            var insertResult = await _context.OrderTrade.BulkWriteAsync(inserts, null, cancellation);
            result = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync(OrderParams specParams, CancellationToken cancellation)
    {
        IEnumerable<Order> result = null!;

        try
        {
            var builder = Builders<Order>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrEmpty(specParams.Search))
            {
                var searchFilter = builder.Regex(x => x.Symbol, new BsonRegularExpression(specParams.Search));
                filter &= searchFilter;
            }

            result = _context.OrderTrade.Find(filter).ToEnumerable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        return result;
    }

    public async Task<bool> UpdateOrderAsync(Order order, CancellationToken cancellation)
    {
        bool result = false;
        try
        {
            var updates = new List<WriteModel<Order>>();
            var filterBuilder = Builders<Order>.Filter;

            var filter = filterBuilder.Where(x => x.OrderID == order.OrderID);
            updates.Add(new ReplaceOneModel<Order>(filter, order));

            var updateResult = await _context.OrderTrade.BulkWriteAsync(updates, null, cancellation);
            result = updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
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
