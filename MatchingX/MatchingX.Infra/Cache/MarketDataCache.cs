using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;
namespace MatchingX.Infra.Cache;
public class MarketDataCache : IMarketDataCache
{
    
    private static ConcurrentQueue<MarketData> IncrementalQueue=null!;
    private static ConcurrentDictionary<string, decimal> _LastPrice;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMatching;
    private readonly ILogger<MarketDataCache> _logger;
    private static long MarketID = 0;
    private RedisKey keyMarketData = new RedisKey("Marketdata");
    
    public MarketDataCache(ILogger<MarketDataCache> logger , IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        IncrementalQueue = new ConcurrentQueue<MarketData>();
        _LastPrice = new ConcurrentDictionary<string, decimal>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    
    public async Task<decimal> GetPrice(string symbol)
    {
        RedisValue value = new RedisValue(symbol);
        var marketHash = await _dbMatching.HashGetAsync(keyMarketData, value);
        if (marketHash.HasValue)
        {
            var marketData = JsonSerializer.Deserialize<MarketData>(marketHash);
            return marketData!.EntryPx;
        }
        return 0;
    }
    public async Task<MarketData> GetMarketDataBySymbol(string symbol)
    {
        var marketData = new MarketData();
        RedisValue value = new RedisValue(symbol);
        var marketHash = await _dbMatching.HashGetAsync(keyMarketData, value);
        if (marketHash.HasValue)
        {
            marketData = JsonSerializer.Deserialize<MarketData>(marketHash);

        }
        return default(MarketData)!;
    }
    private async Task SetValueRedis(MarketData marketData)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MarketData>(marketData));
        
        await _dbMatching.HashSetAsync(keyMarketData, new HashEntry[]
        {
            new HashEntry(marketData.Symbol, value)
        });
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
