using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NRedisStack.Graph;
using QuickFix.Fields;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MarketDataX.Infra.Cache;
public class MarketDataChache : IMarketDataChache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMarketData;
    private readonly ILogger<MarketDataChache> _logger;
    private static ConcurrentQueue<MarketData> IncrementalQueue;
    private RedisKey _key = new RedisKey("Incremental");
    public MarketDataChache(ILogger<MarketDataChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        IncrementalQueue = new ConcurrentQueue<MarketData>();
        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbMarketData = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }
    public async void AddMarketData(MarketData report)
    {
        IncrementalQueue.Enqueue(report);
        await SetValueRedis(report);
    }
    private async Task SetValueRedis(MarketData market)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MarketData>(market));

        await _dbMarketData.HashSetAsync(_key, new HashEntry[]
        {
            new HashEntry(market.Id, value)
        });
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