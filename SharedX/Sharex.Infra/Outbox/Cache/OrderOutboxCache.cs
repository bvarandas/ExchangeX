using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Text.Json;

namespace Sharedx.Infra.Outbox.Cache;
public class OrderOutboxCache : IOrderOutboxCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbOrderStop;
    private readonly ILogger<OrderOutboxCache> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _key = new RedisKey("OrderOutbox");

    public OrderOutboxCache(ILogger<OrderOutboxCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbOrderStop = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
        _key = new RedisKey("OrderOutbox");
    }

    public async Task<bool> DeleteOutboxAsync(string activity, long orderId)
    {
        RedisValue value = new RedisValue(orderId.ToString());
        var key = string.Concat(_key, ":", activity);
        var result = await _dbOrderStop.HashDeleteAsync(key, value);
        return result;
    }

    public async Task<Result<Dictionary<long, OrderEngine>>> GetOutboxAsync(string activity)
    {
        var result = new Dictionary<long, OrderEngine>();
        var key = string.Concat(_key,  ":", activity );

        var hashEntry = await _dbOrderStop.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<OrderEngine>(item.Value!);
            result.Add(long.Parse(item.Name!), value!);
        }

        return Result.Ok(result);
    }

    public async Task<bool> UpsertOutboxAsync(OrderEngine order, string activity)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<OrderEngine>(order));
        var key = string.Concat(_key, ":", activity);
        await _dbOrderStop.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
        return true;
    }
}
