using FluentResults;
using MatchingX.Core.Repositories;
using MatchingX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using QuickFix;
using SharedX.Core.Matching;

namespace MatchingX.Infra.Repositories;

public class TradeRepository : ITradeRepository
{
    private readonly ITradeContext _context;
    private readonly ILogger<TradeRepository> _logger;
    public TradeRepository(ITradeContext context, ILogger<TradeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Trade> GetTradeIdAsync(CancellationToken cancellation)
    {
        Trade result = default(Trade)!;
        try
        {
            var builder = Builders<Trade>.Filter;
            var filter = builder.Empty;

            using (var session = await _context.MongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var single = await _context.Trade.Find(session, filter).SingleAsync(cancellation);
                    result = single;
                    single.TradeId++;

                    var resultReplace = await _context.Trade.ReplaceOneAsync(session,
                        null,
                    replacement: single,
                    options: new ReplaceOptions { IsUpsert = true },
                    cancellation);

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