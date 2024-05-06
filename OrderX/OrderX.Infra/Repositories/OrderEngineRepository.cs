using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OrderEngineX.Core.Interfaces;
using OrderEngineX.Infra.Data;
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
    public async Task<Result<bool>> UpsertOrdersAsync(OrderEngine order, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.OrderID, order.OrderID);
            var single = await _context.OrderEngine.Find( filter).SingleAsync(cancellationToken);

            single.OrderDetails?.Add((OrderEngineDetail)order);

            var resultReplace = await _context.OrderEngine.ReplaceOneAsync(filter,
                replacement: single,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok(result);
    }
}