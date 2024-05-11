using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Text.Json;
namespace Sharex.Infra.Order.Cache;
public class OrderStopCache : IOrderStopCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbOrderStop;
    private readonly ILogger<OrderStopCache> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _key = new RedisKey("OrderStop");

    public OrderStopCache(ILogger<OrderStopCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbOrderStop = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
        _key = new RedisKey("OrderStop");
    }

    public async Task<Result<OrderEngine>> GetOrderByIdandSymbolAsync(long orderId, string symbol)
    {
        var result = new OrderEngine();
        var key = string.Concat(_key, ":", symbol);
        RedisValue value = new RedisValue(orderId.ToString());
        var hashEntry = await _dbOrderStop.HashGetAsync(key, value);

        if (hashEntry.HasValue)
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));

        result = JsonSerializer.Deserialize<OrderEngine>(hashEntry!);
        return Result.Ok(result!);
    }

    public async Task<Result<Dictionary<long, OrderEngine>>> GetOrderBySymbolAsync(string symbol)
    {
        var result = new Dictionary<long, OrderEngine>();
        var key = string.Concat(_key, ":", symbol);
        
        var hashEntry = await _dbOrderStop.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<OrderEngine>(item.Value!);
            result.Add(long.Parse(item.Name!), value!);
        }

        return Result.Ok(result);
    }

    public async void UpsertOrderAsync(OrderEngine order)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<OrderEngine>(order));
        var key = string.Concat(_key, ":", order.Symbol);
        await _dbOrderStop.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
    }

    public async Task<bool> DeleteOrderAsync(string symbol, long orderId)
    {
        RedisValue value = new RedisValue(orderId.ToString());
        var key = string.Concat(_key, ":", symbol);
        var result = await _dbOrderStop.HashDeleteAsync(key, value);
        return result;
    }
}