using MatchingX.Core.Filters;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;

namespace MatchingX.Infra.Repositories;
public class ExecutedTradeRepository : IExecutedTradeRepository
{
    private readonly IExecutedTradeContext _context;
    private readonly ILogger<ExecutedTradeRepository> _logger;
    public ExecutedTradeRepository(IExecutedTradeContext context, ILogger<ExecutedTradeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<bool> CreateExecutedTradeAsync((TradeCaptureReport, TradeCaptureReport) executedTrade, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var inserts = new List<WriteModel<TradeCaptureReport>>();

            inserts.Add(new InsertOneModel<TradeCaptureReport>(executedTrade.Item1));
            inserts.Add(new InsertOneModel<TradeCaptureReport>(executedTrade.Item2));

            var insertResult = await _context.ExecutedTrade.BulkWriteAsync(inserts,null, cancellationToken);
            result = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }
    public async Task<bool> CreateExecutedTradesAsync(List<TradeCaptureReport> executedTrades, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var inserts = new List<WriteModel<TradeCaptureReport>>();

            executedTrades.ForEach((executedTrade)=>
            {
                inserts.Add(new InsertOneModel<TradeCaptureReport>(executedTrade));
            });

            var insertResult = await _context.ExecutedTrade.BulkWriteAsync(inserts, null ,cancellationToken);
            result = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return result;
    }
    public async Task<IEnumerable<TradeCaptureReport>> GetExecutedTradeAsync(ExecutedTradeParams specParams, CancellationToken cancellationToken)
    {
        IEnumerable<TradeCaptureReport> result = null!;

        try
        {
            var builder = Builders<TradeCaptureReport>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrEmpty(specParams.Search))
            {
                var searchFilter = builder.Regex(x => x.Id, new BsonRegularExpression(specParams.Search));
                filter &= searchFilter;
            }

            result = _context.ExecutedTrade.Find(filter).ToEnumerable();
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