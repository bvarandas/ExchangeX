using FluentResults;
using MarketDataX.Core.Interfaces;
using MarketDataX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Infra.Repositories;
public class MarketDataRepository : IMarketDataRepository
{
    private readonly IMarketDataContext _context;
    private readonly ILogger<MarketDataRepository> _logger;

    public MarketDataRepository(IMarketDataContext context, ILogger<MarketDataRepository> logger)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Result> UpsertMarketDataSnapshotAsync(MarketDataSnapshot marketDataSnapshot, CancellationToken cancellation)
    {
        bool resultUpsert = false;
        Result result = null!;
        try
        {
            var builder = Builders<MarketDataSnapshot>.Filter;
            var filter = builder.Eq(o => o.Id, marketDataSnapshot.Id);

            var resultReplace = await _context.MarketDataSnapshot.ReplaceOneAsync(
            filter,
            replacement: marketDataSnapshot,
            options: new ReplaceOptions { IsUpsert = true },
            cancellation);

            resultUpsert = resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0;
            if (resultUpsert)
                result = Result.Ok();
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex.Message, ex);
            result = Result.Fail(new Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            result = Result.Fail(new Error(ex.Message));
        }
        
        return result;
    }


    public async Task<Result<MarketDataSnapshot>> GetSnapshotAsync(string symbol,DateTime date , CancellationToken cancellation)
    {
        Result<MarketDataSnapshot> result = null!;
        MarketDataSnapshot resultSnapshot = null!;
        try
        {
            string formatDate = "yyyyMMdd";
            var builder = Builders<MarketDataSnapshot>.Filter;
            var filter = builder.Eq(o => o.Symbol, symbol) & 
                         builder.Eq(o=>o.LastUpdateTime.ToString(formatDate), date.ToString(formatDate));

            resultSnapshot = await _context.MarketDataSnapshot.Find(filter).SingleAsync(cancellation);
            result = Result.Ok(resultSnapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            result = Result.Fail(new Error(ex.Message));
        }

        return result;
    }

    public async Task<Result<Dictionary<string, MarketDataSnapshot>>> GetAllSnapshotAsync(string symbol, CancellationToken cancellation)
    {
        Dictionary<string, MarketDataSnapshot> result = null!;
        try
        {
            var builder = Builders<MarketDataSnapshot>.Filter;
            var filter = builder.Eq(o => o.Symbol, symbol);
            var snapshots = _context.MarketDataSnapshot.Find(filter).ToEnumerable();
            
            result = new Dictionary<string, MarketDataSnapshot>();

            foreach (var snapshot in snapshots)
                result.Add(snapshot.Id , snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return Result.Ok(result);
    }
}