using OrderEntryX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using SharedX.Core;
using SharedX.Core.Matching;
using Order = SharedX.Core.Matching.Order;

namespace DropCopyX.Infra.Cache;
public class OrderChache : IOrderChache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbOrderEntry;
    private readonly ILogger<OrderChache> _logger;
    private static ConcurrentQueue<Order> IncrementalQueue;
    private RedisKey key = new RedisKey(Constants.RedisOrderEngine);

    public OrderChache(ILogger<OrderChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        IncrementalQueue = new ConcurrentQueue<Order>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbOrderEntry = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    public async void AddMarketData(Order order)
    {
        IncrementalQueue.Enqueue(order);
        await SetValueRedis(order);
    }

    private async Task SetValueRedis(Order order)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(order));
        await _dbOrderEntry.SetAddAsync(key, value);
    }

    public Order TryDequeueExecutionReport(ref Order order)
    {
        if (IncrementalQueue.TryDequeue(out Order orderFound))
        {
            order = orderFound;
            return order;
        }
        return default(Order);
    }

}