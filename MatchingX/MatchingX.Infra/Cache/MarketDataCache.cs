using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
namespace MatchingX.Infra.Cache;
public class MarketDataCache : IMarketDataCache
{
    private static ConcurrentQueue<Security> SecurityQueue;
    private static ConcurrentQueue<MarketData> IncrementalQueue;
    
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMatching;
    private readonly ILogger<MarketDataCache> _logger;
    private static long MarketID = 0;
    private RedisKey keyMarketData = new RedisKey("Marketdata");
    private RedisKey keySecurity = new RedisKey("Security");
    public MarketDataCache(ILogger<MarketDataCache> logger , IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        IncrementalQueue = new ConcurrentQueue<MarketData>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    public async void AddSecurity(Security security)
    {
        SecurityQueue.Enqueue(security);
        await SetValueRedis(security);
    }

    private async Task SetValueRedis(Security security)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(security));
        await _dbMatching.HashIncrementAsync(keySecurity, value);
    }

    private async Task SetValueRedis(MarketData marketData)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(marketData));
        await _dbMatching.HashIncrementAsync(keyMarketData, value);
    }

    public bool TryDequeueMarketData(out MarketData marketData)
    {
        marketData = default(MarketData);
        if (IncrementalQueue.TryDequeue(out MarketData marketDataFound))
        {
            marketData = marketDataFound;
            return true;
        }
        return false;
    }

    public async void AddIncremental(MarketData marketData)
    {
        marketData.Id = ++MarketID;
        IncrementalQueue.Enqueue(marketData);
        await SetValueRedis(marketData);
    }
}
