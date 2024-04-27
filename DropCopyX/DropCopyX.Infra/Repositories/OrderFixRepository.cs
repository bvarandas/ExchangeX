using DropCopyX.Core.Filters;
using DropCopyX.Core.Interfaces;
using DropCopyX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedX.Core.Matching;
namespace DropCopyX.Infra.Repositories;
public class OrderFixRepository : IOrderFixRepository
{
    private readonly IOrderFixContext _context;
    private readonly ILogger<OrderFixRepository> _logger;
    public OrderFixRepository(IOrderFixContext context, ILogger<OrderFixRepository> logger)
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

    public async Task<IEnumerable<Order>> GetOrdersAsync(OrderFixParams specParams, CancellationToken cancellation)
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
