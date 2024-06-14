using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SharedX.Core.Matching.DropCopy;
using TradeEngineX.Core.Interfaces;
using TradeEngineX.Infra.Data;

namespace TradeEngineX.Infra.Repositories;
public class TradeEngineRepository : ITradeEngineRepository
{
    private readonly ITradeEngineContext _context;
    private readonly ILogger<TradeEngineRepository> _logger;

    public TradeEngineRepository(ITradeEngineContext context, ILogger<TradeEngineRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<Result<Dictionary<long, TradeReport>>> GetAllTodayTradesAsync(CancellationToken cancellation)
    {
        Dictionary<long, TradeReport> result = null!;
        try
        {
            string dataFilter = DateTime.Now.ToString("yyyyMMdd");
            var builder = Builders<TradeReport>.Filter;
            var filter = builder.Eq(o => o.DataTrade.ToString("yyyyMMdd"), dataFilter);
            var trades = _context.TradeEngine.Find(filter).ToEnumerable();

            foreach (var security in trades)
                result.Add(security.TradeId, security);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok(result);
    }

    public async Task<Result<TradeReport>> GetTradeAsync(long tradeId, CancellationToken cancellation)
    {
        TradeReport result = null!;
        try
        {
            string dataFilter = DateTime.Now.ToString("yyyyMMdd");
            var builder = Builders<TradeReport>.Filter;
            var filter = builder.Eq(o => o.TradeId, tradeId);
            result = await _context.TradeEngine.Find(filter).SingleAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok(result);
    }

    public async Task<Result> UpsertTradeAsync(TradeReport trade, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<TradeReport>.Filter;
            var filter = builder.Eq(o => o.TradeId, trade.TradeId);

            var resultReplace = await _context.TradeEngine.ReplaceOneAsync(filter,
                replacement: trade,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }

    public async Task<Result> RemoveTradeAsync(TradeReport trade, CancellationToken cancellationToken)
    {
        try
        {
            var builder = Builders<TradeReport>.Filter;
            var filter = builder.Eq(o => o.TradeId, trade.TradeId);

            var resultReplace = await _context.TradeEngine.ReplaceOneAsync(filter,
                replacement: trade,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }
}