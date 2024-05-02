using OrderEntryX.Infra.Data;
using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OrderEntryX.Core.Repositories;
using SharedX.Core.Matching;
using OrderEntryX.Core.Entities;

namespace OrderEntryX.Infra.Repositories;
public class OrderEntryRepository : IOrderEntryRepository
{
    private readonly IOrderEntryContext _context;
    private readonly ILogger<OrderEntryRepository> _logger;

    public OrderEntryRepository(IOrderEntryContext context, ILogger<OrderEntryRepository> logger)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<bool> CreateOrdersAsync(OrderEntry order, CancellationToken cancellationToken)
    {
        Result<bool> result = false;
        try
        {
            var inserts = new List<WriteModel<OrderEntry>>();
            //foreach (var order in orders)
            inserts.Add(new InsertOneModel<OrderEntry>(order));

            var insertResult = await _context.OrderEntry.BulkWriteAsync(inserts, null, cancellationToken);
            result = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new Error(ex.Message));
            _logger.LogError(ex.Message, ex);
        }
        return result.IsSuccess;
    }
}