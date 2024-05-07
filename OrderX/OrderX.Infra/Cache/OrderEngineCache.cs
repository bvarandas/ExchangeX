using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Enums;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using OrderEng = SharedX.Core.Matching.OrderEng;

namespace OrderEngineX.Infra.Cache;

public class OrderEngineCache : IOrderEngineCache
{
    private readonly ConcurrentQueue<OrderEng> OrderEngineQueue;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbOrderEngine;
    private readonly ILogger<OrderEngineCache> _logger;
        private readonly RedisKey _key;
    public OrderEngineCache(ILogger<OrderEngineCache> logger, 
        IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        OrderEngineQueue = new ConcurrentQueue<OrderEng>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbOrderEngine = _redis.GetDatabase((int)RedisDataBases.OrderEngine);
    
        _logger = logger;

        _key = new RedisKey("Orders");

    }
    public async void AddOrder(OrderEng order)
    {
        OrderEngineQueue.Enqueue(order);
        await SetValueOrderRedisAsync(order);
    }

    private async Task SetValueOrderRedisAsync(OrderEng order)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(order));
        await _dbOrderEngine.HashIncrementAsync(_key, value);
    }

    public bool TryDequeueOrder(out OrderEng order)
    {
        order = default;
        if (OrderEngineQueue.TryDequeue(out OrderEng orderFound))
        {
            order = orderFound;
            return true;
        }
        return false;
    }
}
