using FluentResults;
using MatchingX.Core.Entities;
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
public class MatchingCache : IMatchingCache
{
    private readonly ConcurrentQueue<MarketData> IncrementalQueue = null!;
    private readonly ConcurrentDictionary<string, decimal> _LastPrice;

    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMatching;
    private readonly ILogger<MatchingCache> _logger;

    private static long MarketID = 0;

    private RedisKey keyBuy = new RedisKey("order_to_match_buy");
    private RedisKey keySell = new RedisKey("order_to_match_sell");

    public MatchingCache(ILogger<MatchingCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        IncrementalQueue = new ConcurrentQueue<MarketData>();
        _LastPrice = new ConcurrentDictionary<string, decimal>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options =>
        {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    public async Task<Result> UpsertOrderMatchingAsync(MatchingEngine matchEngine, CancellationToken cancellation)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MatchingEngine>(matchEngine));

        var keyRedis = (matchEngine.PrincipalOrder.Side == SideTrade.Buy) ? keyBuy : keySell;

        var key = string.Concat(keyRedis, ":", matchEngine.PrincipalOrder.Symbol);

        await _dbMatching.HashSetAsync(key,
            new HashEntry[]{
                new HashEntry(matchEngine.PrincipalOrder.OrderID, value)
            });

        return Result.Ok();
    }
    public async Task<Result<Dictionary<long, MatchOrder>>> GetBuyOrderBySymbol(string symbol)
    {
        var result = new Dictionary<long, MatchOrder>();
        var key = string.Concat(keyBuy, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);

        //hashEntry.MaxBy(c=>c.Value.)
        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<MatchOrder>(item.Value);
            result.Add(long.Parse(item.Name), value);
        }
        var ordered = result.OrderByDescending(o => o.Value.Price);

        result = ordered.ToDictionary<KeyValuePair<long, MatchOrder>, long, MatchOrder>
            (pair => pair.Key, pair => pair.Value);

        return Result.Ok(result);
    }
    public async Task<Result<Dictionary<long, MatchOrder>>> GetSellOrderBySymbol(string symbol)
    {
        var result = new Dictionary<long, MatchOrder>();
        var key = string.Concat(keySell, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);
        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<MatchOrder>(item.Value);
            result.Add(long.Parse(item.Name), value);
        }
        var ordered = result.OrderBy(o => o.Value.Price);

        result = ordered.ToDictionary<KeyValuePair<long, MatchOrder>, long, MatchOrder>
            (pair => pair.Key, pair => pair.Value);

        return Result.Ok(result);
    }



    public async Task<Result<bool>> RemoveOrderMatchingAsync(string symbol, long orderId, SideTrade side)
    {
        var keyRedis = (side == SideTrade.Buy) ? keyBuy : keySell;

        var key = string.Concat(keyRedis, ":", symbol);

        RedisValue value = new RedisValue(orderId.ToString());
        var hashResult = await _dbMatching.HashDeleteAsync(key, value);

        return Result.Ok(hashResult);
    }
}