using OrderEntryX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
namespace DropCopyX.Infra.Cache;
public class OrderChache : IOrderChache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbOrderEntry;
    private readonly ILogger<OrderChache> _logger;

    private static ConcurrentQueue<MarketData> IncrementalQueue;
    public OrderChache(ILogger<OrderChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        IncrementalQueue = new ConcurrentQueue<MarketData>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbOrderEntry = _redis.GetDatabase((int)RedisDataBases.MatchingExecutionReport);

        _logger = logger;
        
    }

    public async void AddMarketData(MarketData report)
    {
        IncrementalQueue.Enqueue(report);
        await SetValueRedis(report);
    }

    private async Task SetValueRedis(MarketData market)
    {
        RedisKey key = new RedisKey(market.Id.ToString());
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(market));
        await _dbOrderEntry.SetAddAsync(key, value);
    }

    public MarketData TryDequeueExecutionReport()
    {
        if (IncrementalQueue.TryDequeue(out MarketData report))
        {
            return report;
        }
        return default(MarketData);
    }

}