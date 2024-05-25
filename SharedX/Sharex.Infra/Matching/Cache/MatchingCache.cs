using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using StackExchange.Redis;
using FluentResults;
using System.Text.Json;
using SharedX.Core.Interfaces;

namespace SharedX.Infra.Cache;
public class MatchingCache : IMatchingCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbMatching;
    private readonly ILogger<MatchingCache> _logger;
    private readonly ConnectionMultiplexer _redis;

    private RedisKey keyBuyOrders = new RedisKey("BuyOrders");
    private RedisKey keySellOrders = new RedisKey("SellOrders");

    public MatchingCache(ILogger<MatchingCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }
    public async Task<Result<OrderEngine>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol)
    {
        var result = new OrderEngine();
        var key = string.Concat(keyBuyOrders, ":", symbol);
        RedisValue value = new RedisValue(orderId.ToString());
        var hashEntry = await _dbMatching.HashGetAsync(key, value);
        
        if ( hashEntry.HasValue )
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));
        
        result = JsonSerializer.Deserialize<OrderEngine>(hashEntry!);
        return Result.Ok(result);
    }

    public async Task<Result<OrderEngine>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol)
    {
        var result = new OrderEngine();
        var key = string.Concat(keySellOrders, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);

        var order = hashEntry.GetValue(orderId);

        if (order is null)
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));

        result = JsonSerializer.Deserialize<OrderEngine>(order?.ToString());
        
        return Result.Ok(result);
    }

    public async Task<Result<Dictionary<long, OrderEngine>>> GetBuyOrderBySymbol(string symbol)
    {
        var result =new Dictionary<long, OrderEngine>();
        var key = string.Concat(keyBuyOrders, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);

        //hashEntry.MaxBy(c=>c.Value.)
        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<OrderEngine>(item.Value);
            result.Add(long.Parse(item.Name), value);
        }
        return Result.Ok(result);
    }

    public async Task<Result<Dictionary<long, OrderEngine>>> GetSellOrderBySymbol(string symbol)
    {
        var result = new Dictionary<long, OrderEngine>();
        var key = string.Concat(keySellOrders, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);
        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<OrderEngine>(item.Value);
            result.Add(long.Parse(item.Name), value);
        }
        return Result.Ok(result);
    }
    
    public async Task<bool> UpsertBuyOrder(OrderEngine order)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<OrderEngine>(order));
        var key = string.Concat(keyBuyOrders, ":", order.Symbol);
        await _dbMatching.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
        return true;
    }

    public async Task<bool> UpsertSellOrder(OrderEngine order)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<OrderEngine>(order));
        var key = string.Concat(keySellOrders, ":", order.Symbol);
        await _dbMatching.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
        return true;
    }

    public async Task<bool> DeleteBuyOrderAsync(string symbol, long orderId)
    {
        RedisValue value = new RedisValue(orderId.ToString());
        
        var key = string.Concat(keyBuyOrders, ":", symbol);
        var result = await _dbMatching.HashDeleteAsync(key, value);
        return result;
    }

    public async Task<bool> DeleteSellOrderAsync(string symbol, long orderId)
    {
        RedisValue value = new RedisValue(orderId.ToString());
        var key = string.Concat(keySellOrders, ":", symbol);
        var result = await _dbMatching.HashDeleteAsync(key, value);
        return result;
    }
    public async Task<bool> DeleteAllOrderAsync(Dictionary<long, OrderEngine> dicOrders)
    {
        bool result = false;
        foreach (var order in dicOrders.Values)
        {
            RedisValue value = new RedisValue(order.OrderID.ToString());
            var key = string.Concat(keySellOrders, ":", order.Symbol);
            result = await _dbMatching.HashDeleteAsync(key, value);
        }
        return result;
    }

}