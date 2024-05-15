using MatchingX.Core.Interfaces;
using MatchingX.Infra.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Enums;
using StackExchange.Redis;

namespace MatchingX.Infra.Repositories;

public class MatchingRepository : IMatchingRepository
{
    private readonly IMatchingContext _context;
    private readonly ILogger<MatchingRepository> _logger;
    public MatchingRepository(IMatchingContext context, ILogger<MatchingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Trade> MatchingLimitAsync(OrderEngine orderEngine,  CancellationToken cancellation)
    {
        Trade result = default(Trade)!;
        try
        {
            var builder = Builders<OrderEngine>.Filter;
            var filter = builder.Eq(o => o.Symbol, orderEngine.Symbol);
            var sort = Builders<OrderEngine>.Sort.Ascending("Price");

            using (var session = await _context.MongoClient.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    if (orderEngine.Side == SideTrade.Buy)
                    {
                        filter = filter & builder.Eq(o=>o.Side, SideTrade.Sell);
                        sort = Builders<OrderEngine>.Sort.Ascending("Price");
                    }
                    else if (orderEngine.Side == SideTrade.Sell)
                    {
                        filter =filter &  builder.Eq(o => o.Side, SideTrade.Buy);
                        sort = Builders<OrderEngine>.Sort.Descending("Price");
                    }

                    if (orderEngine.TimeInForce != TimeInForce.FOK)
                    {
                        filter = filter & builder.Gte(o => o.Price, orderEngine.Price);
                        filter = filter & builder.Eq(o=>o.LeavesQuantity ,orderEngine.Quantity);
                    }else if(orderEngine.TimeInForce == TimeInForce.FOK)
                    {
                        filter = filter & builder.Gte(o => o.Price, orderEngine.Price);
                        //filter = filter & builder.
                    }

                    var orders = await _context.Matching.FindAsync(session, filter,
                        new FindOptions<OrderEngine, OrderEngine>
                        {
                            Sort = sort,
                        });
                    


                    //var resultReplace = await _context.Matching.ReplaceOneAsync(session,
                    //    null,
                    //replacement: single,
                    //options: new ReplaceOptions { IsUpsert = true },
                    //cancellation);

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
