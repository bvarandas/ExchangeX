using OrderEntryX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using SharedX.Core;
using SharedX.Core.Matching.OrderEngine;
using System.Text.Json;

namespace DropCopyX.Infra.Cache;
public class OrderEntryChache : IOrderEntryChache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbOrderEntry;
    private readonly ILogger<OrderEntryChache> _logger;
    private static ConcurrentQueue<OrderEngine> OrderEntryQueue=null!;
    private RedisKey key = new RedisKey(Constants.RedisOrderEngine);

    public OrderEntryChache(ILogger<OrderEntryChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        OrderEntryQueue = new ConcurrentQueue<OrderEngine>();
        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbOrderEntry = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }
    public async void AddOrderEntryAsync(OrderEngine order)
    {
        OrderEntryQueue.Enqueue(order);
        await SetValueRedis(order);
    }
    private async Task SetValueRedis(OrderEngine order)
    {
        var jsonString = JsonSerializer.Serialize<OrderEngine>(order);
        RedisValue value = new RedisValue();
        await _dbOrderEntry.SetAddAsync(key, value);
    }
    public bool TryDequeueOrderEntry(out OrderEngine order)
    {
        order = default!;
        if (OrderEntryQueue.TryDequeue(out OrderEngine orderFound))
        {
            order = orderFound;
            return true;
        }
        return false;
    }
}