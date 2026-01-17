using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OrderEngineX.Infra.Cache;

public class OrderEngineCache : IOrderEngineCache
{
    private readonly ConcurrentQueue<OrderEngine> OrderEngineQueue;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbOrderEngine;
    private readonly ILogger<OrderEngineCache> _logger;
    private readonly RedisKey _key;

    public OrderEngineCache(ILogger<OrderEngineCache> logger,
        IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        OrderEngineQueue = new ConcurrentQueue<OrderEngine>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options =>
        {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbOrderEngine = _redis.GetDatabase((int)RedisDataBases.OrderEngine);
        _logger = logger;
        _key = new RedisKey("OrderEngine");
    }

    public async Task<Result<Dictionary<string, OrderEngine>>> GetOrdersBySymbolAsync(string symbol, DateTime date)
    {
        var result = new Dictionary<string, OrderEngine>();
        var key = string.Concat(_key, ":", date.ToString("yyyyMMdd"));
        var hashEntry = await _dbOrderEngine.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<OrderEngine>(item.Value);
            result.Add(item.Name, value);
        }

        var ordersBySymbol = result
            .Where(d => d.Value.Symbol == symbol)
            .ToDictionary(i => i.Key, i => i.Value);

        return Result.Ok(ordersBySymbol);
    }

    public async Task<Result<Dictionary<string, OrderEngine>>> GetOrdersAsync(DateTime date)
    {
        var result = new Dictionary<string, OrderEngine>();
        var key = string.Concat(_key, ":", date.ToString("yyyyMMdd"));
        var hashEntry = await _dbOrderEngine.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<OrderEngine>(item.Value);
            result.Add(item.Name, value);
        }
        return Result.Ok(result);
    }

    public async void AddOrder(OrderEngine order)
    {
        OrderEngineQueue.Enqueue(order);
        await SetValueOrderRedisAsync(order);
    }

    private async Task SetValueOrderRedisAsync(OrderEngine order)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<OrderEngine>(order));
        var key = string.Concat(_key, ":", DateTime.Now.ToString("yyyyMMdd"));

        await _dbOrderEngine.HashSetAsync(key, new HashEntry[]
            {
                new HashEntry(order.OrderID.ToString(), value)
            });
    }

    public bool TryDequeueOrder(out OrderEngine order)
    {
        order = default;
        if (OrderEngineQueue.TryDequeue(out OrderEngine orderFound))
        {
            order = orderFound;
            return true;
        }
        return false;
    }

    public async Task<Result<long>> GetOrderIdAsync(CancellationToken cancellation)
    {
        var key = string.Concat(_key, ":", "id");
        var hashEntry = await _dbOrderEngine.HashGetAllAsync(key);
        var item = hashEntry?.FirstOrDefault();
        long orderId = 1;

        if (item.HasValue)
        {
            var value = JsonSerializer.Deserialize<OrderIDEngine>(item.Value.Value!);
            value.OrderId++;
            orderId = value.OrderId;

            await _dbOrderEngine.HashSetAsync(key, new HashEntry[]
            {
                new HashEntry("1", orderId)
            });

        }
        else
        {
            await _dbOrderEngine.HashSetAsync(key, new HashEntry[]
            {
                new HashEntry("1", orderId)
            });
        }

        return Result.Ok(orderId);
    }
}