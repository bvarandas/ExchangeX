using FluentResults;
using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    
    private RedisKey _keyIncremental = new RedisKey("Incremental");
    
    public MarketDataChache(ILogger<MarketDataChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        
        IncrementalQueue = new ConcurrentQueue<MarketData>();
        
        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbMarketData = _redis.GetDatabase((int)RedisDataBases.Fix);
        
        _logger = logger;
    }
    public async void AddMarketDataIncremental(MarketData report)
    {
        IncrementalQueue.Enqueue(report);
        await SetValueIncrementalRedis(report);
    }
    private async Task SetValueIncrementalRedis(MarketData market)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MarketData>(market));

        await _dbMarketData.HashSetAsync(_keyIncremental, new HashEntry[]
        {
            new HashEntry(market.Id, value)
        });
    }

    public async Task<bool> AddMarketDataBook(MarketData marketData)
    {
        var bookAdded = await SetValueBookRedis(marketData);
        return bookAdded;
    }

    private async Task<bool> SetValueBookRedis(MarketData market)
    {
        RedisValue value = new RedisValue(market.EntryType.ToString());
        RedisKey key = new RedisKey(string.Concat(_keyIncremental, ":", market.Symbol));

        await _dbMarketData.HashSetAsync(key, new HashEntry[]
        {
            new HashEntry(market.PriceLevel, value)
        });
        return true;
    }

    public async Task<Result<Dictionary<long, MarketData>>> GetSnapShotMarketData()
    {
        var result = new Dictionary<long, MarketData>();
        var snap = await _dbMarketData.HashGetAllAsync(_keyIncremental);

        foreach (var data in snap)
            result.TryAdd(long.Parse(data.Name!), JsonSerializer.Deserialize<MarketData>(data.Value!)!);
        
        return Result.Ok(result);
    }
    public bool TryDequeueMarketData(out MarketData marketData)
    {
        marketData = default(MarketData)!;
        if (IncrementalQueue.TryDequeue(out MarketData report))
        {
            marketData = report;
            return true;
        }
        return false;
    }

    
}