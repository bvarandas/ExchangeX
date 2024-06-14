using FluentResults;
using MatchingX.Core.Filters;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;
using StackExchange.Redis;

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

    public async Task<Result> CreateExecutedTradeAsync( Dictionary<long, TradeReport> dicExecutedTrade, CancellationToken cancellationToken)
    {
        Result result = null!;
        bool resultInsert = false;
        try
        {
            var inserts = new List<WriteModel<TradeReport>>();
            
            foreach (var trade in dicExecutedTrade)
                inserts.Add(new InsertOneModel<TradeReport>(trade.Value));
            
            var insertResult = await _context.ExecutedTrade.BulkWriteAsync(inserts, null, cancellationToken);
            resultInsert = insertResult.IsAcknowledged && insertResult.ModifiedCount > 0;
            if (resultInsert)
            {
                result = Result.Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            result = Result.Fail(new Error(ex.Message));
        }
        return result;
    }
    public async Task<Result<IEnumerable<TradeReport>>> GetExecutedTradeAsync(ExecutedTradeParams specParams, CancellationToken cancellationToken)
    {
        IEnumerable<TradeReport> result = null!;

        try
        {
            var builder = Builders<TradeReport>.Filter;
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
            return Result.Fail(new Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }

        return Result.Ok( result);
    }
}