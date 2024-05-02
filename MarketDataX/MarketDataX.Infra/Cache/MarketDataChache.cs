using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
namespace MarketDataX.Infra.Cache;
public class MarketDataChache : IMarketDataChache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMarketData;
    private readonly ILogger<MarketDataChache> _logger;
    private static ConcurrentQueue<MarketData> IncrementalQueue;
    public MarketDataChache(ILogger<MarketDataChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        IncrementalQueue = new ConcurrentQueue<MarketData>();
        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbMarketData = _redis.GetDatabase((int)RedisDataBases.MatchingExecutionReport);
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
        await _dbMarketData.SetAddAsync(key, value);
    }
    public MarketData TryDequeueMarketData()
    {
        if (IncrementalQueue.TryDequeue(out MarketData report))
        {
            return report;
        }
        return default(MarketData);
    }
}