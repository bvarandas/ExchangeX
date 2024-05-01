using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using Order = SharedX.Core.Matching.Order;

namespace MatchingX.Infra.Cache;

public class MarketDataCache : IMarketDataCache
{
    private static ConcurrentQueue<Security> SecurityQueue;
    private static ConcurrentQueue<MarketData> IncrementalQueue;
    
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbSecurity;
    private readonly IDatabase _dbSnapshotIncremental;
    private readonly ILogger<MarketDataCache> _logger;
    private static long MarketID = 0;

    public MarketDataCache(ILogger<MarketDataCache> logger , IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        IncrementalQueue = new ConcurrentQueue<MarketData>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbSecurity = _redis.GetDatabase((int)RedisDataBases.MatchingSecurity);
        _dbSnapshotIncremental = _redis.GetDatabase((int)RedisDataBases.MatchingSnapshotIncrement);
        _logger = logger;
    }

    public async void AddSecurity(Security security)
    {
        SecurityQueue.Enqueue(security);
        await SetValueRedis(security);
    }

    private async Task SetValueRedis(Security security)
    {
        RedisKey key = new RedisKey(security.SecurityID);
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(security));
        await _dbSecurity.SetAddAsync(key, value);
    }

    private async Task SetValueRedis(MarketData marketData)
    {

        RedisKey key = new RedisKey(marketData.Id.ToString());
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(marketData));
        await _dbSnapshotIncremental.SetAddAsync(key, value);
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
