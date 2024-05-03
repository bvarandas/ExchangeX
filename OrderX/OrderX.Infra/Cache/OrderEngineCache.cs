using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderEngineX.Core.Interfaces;
using QuickFix.Config;
using ServiceStack.Redis;
using SharedX.Core.Enums;
using SharedX.Core.Matching;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using Order = SharedX.Core.Matching.Order;

namespace OrderEngineX.Infra.Cache;

public class OrderEngineCache : IOrderEngineCache
{
    private readonly ConcurrentQueue<Order> OrderEngineQueue;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbOrderEngine;
    private readonly ILogger<OrderEngineCache> _logger;
    private readonly RedisClient _redisClient;
    private readonly RedisKey _key;
    public OrderEngineCache(ILogger<OrderEngineCache> logger, 
        IOptions<ConnectionRedis> config,
        RedisClient redisClient)
    {
        _config = config.Value;
        OrderEngineQueue = new ConcurrentQueue<Order>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbOrderEngine = _redis.GetDatabase((int)RedisDataBases.OrderEngine);
        _redisClient = redisClient;
        _logger = logger;

        _key = new RedisKey("Orders");

    }
    public async void AddOrder(Order order)
    {
        OrderEngineQueue.Enqueue(order);
        await SetValueOrderRedisAsync(order);
    }

    private async Task SetValueOrderRedisAsync(Order order)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(order));
        await _dbOrderEngine.HashIncrementAsync(_key, value);
    }

    public bool TryDequeueOrder(ref Order order)
    {
        if (OrderEngineQueue.TryDequeue(out Order orderFound))
        {
            order = orderFound;
            return true;
        }
        return false;
    }
}
