using FluentResults;
using FluentValidation;
using MarketDataX.Core.Interfaces;
using MarketDataX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using QuickFix.Fields;
using SharedX.Core.Entities;
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

    public async Task<bool> UpsertMarketDataSnapshotAsync(MarketDataSnapshot marketDataSnapshot, CancellationToken cancellation)
    {
        bool result = false;
        
        try
        {
            var builder = Builders<MarketDataSnapshot>.Filter;
            var filter = builder.Eq(o => o.Id, marketDataSnapshot.Id);

            var resultReplace = await _context.MarketDataSnapshot.ReplaceOneAsync(
            filter,
            replacement: marketDataSnapshot,
            options: new ReplaceOptions { IsUpsert = true },
            cancellation);

            result = resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0;
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


    public async Task<Result<MarketDataSnapshot>> GetSnapshotAsync(string symbol,DateTime date , CancellationToken cancellation)
    {
        MarketDataSnapshot result = null!;
        try
        {
            string formatDate = "yyyyMMdd";
            var builder = Builders<MarketDataSnapshot>.Filter;
            var filter = builder.Eq(o => o.Symbol, symbol) & 
                         builder.Eq(o=>o.LastUpdateTime.ToString(formatDate), date.ToString(formatDate));

            result = await _context.MarketDataSnapshot.Find(filter).SingleAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        return Result.Ok(result);
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