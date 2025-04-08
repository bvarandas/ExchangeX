using FluentResults;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchingX.Infra.Cache;

public class OrderStopCache : IOrderStopCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbOrderStop;
    private readonly ILogger<OrderStopCache> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _keyBuy = new RedisKey("stop_buy");
    private readonly RedisKey _keySell = new RedisKey("stop_sell");

    public OrderStopCache(ILogger<OrderStopCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options =>
        {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbOrderStop = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    public async Task<Result<MatchOrder>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol)
    {
        var result = new MatchOrder();
        var key = $"{_keyBuy}:{symbol}";
        RedisValue value = new RedisValue(orderId.ToString());
        var hashEntry = await _dbOrderStop.HashGetAsync(key, value);

        if (hashEntry.HasValue)
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));

        result = JsonSerializer.Deserialize<MatchOrder>(hashEntry!);
        return Result.Ok(result!);
    }

    public async Task<Result<MatchOrder>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol)
    {
        var result = new MatchOrder();
        var key = $"{_keySell}:{symbol}";
        RedisValue value = new RedisValue(orderId.ToString());
        var hashEntry = await _dbOrderStop.HashGetAsync(key, value);

        if (hashEntry.HasValue)
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));

        result = JsonSerializer.Deserialize<MatchOrder>(hashEntry!);
        return Result.Ok(result!);
    }

    public async Task<Result<Dictionary<long, MatchOrder>>> GetBuyOrderBySymbolAsync(string symbol)
    {
        var result = new Dictionary<long, MatchOrder>();
        var key = $"{_keyBuy}:{symbol}";

        var hashEntry = await _dbOrderStop.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<MatchOrder>(item.Value!);
            result.Add(long.Parse(item.Name!), value!);
        }

        return Result.Ok(result);
    }

    public async Task<Result<Dictionary<long, MatchOrder>>> GetSellOrderBySymbolAsync(string symbol)
    {
        var result = new Dictionary<long, MatchOrder>();
        var key = $"{_keySell}:{symbol}";

        var hashEntry = await _dbOrderStop.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<MatchOrder>(item.Value!);
            result.Add(long.Parse(item.Name!), value!);
        }

        return Result.Ok(result);
    }

    public async Task UpsertBuyOrderAsync(MatchOrder order)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MatchOrder>(order));
        var key = $"{_keyBuy}:{order.Symbol}";
        await _dbOrderStop.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
    }

    public async Task UpsertSellOrderAsync(MatchOrder order)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MatchOrder>(order));
        var key = $"{_keySell}:{order.Symbol}";
        await _dbOrderStop.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
    }

    public async Task<bool> DeleteBuyOrderAsync(string symbol, long orderId)
    {
        RedisValue value = new RedisValue(orderId.ToString());
        var key = $"{_keyBuy}:{symbol}";
        var result = await _dbOrderStop.HashDeleteAsync(key, value);
        return result;
    }

    public async Task<bool> DeleteSellOrderAsync(string symbol, long orderId)
    {
        RedisValue value = new RedisValue(orderId.ToString());
        var key = $"{_keySell}:{symbol}";
        var result = await _dbOrderStop.HashDeleteAsync(key, value);
        return result;
    }

}
