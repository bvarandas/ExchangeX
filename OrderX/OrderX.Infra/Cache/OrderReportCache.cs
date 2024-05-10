using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OrderEngineX.Infra.Cache;
public class OrderReportCache : IOrderReportCache
{
    private readonly ConcurrentQueue<ReportFix> OrderResponseQueue;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbOrderReport;
    private readonly ILogger<OrderReportCache> _logger;
    private readonly RedisKey _key;
    public OrderReportCache(ILogger<OrderReportCache> logger,
        IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        OrderResponseQueue = new ConcurrentQueue<ReportFix>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbOrderReport = _redis.GetDatabase((int)RedisDataBases.OrderReport);

        _logger = logger;

        _key = new RedisKey("OrderReport");

    }
    public async void AddReport(ReportFix order)
    {
        OrderResponseQueue.Enqueue(order);
        await SetValueOrderRedisAsync(order);
    }
    private async Task SetValueOrderRedisAsync(ReportFix report)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<ReportFix>(report));
        await _dbOrderReport.HashSetAsync(_key, new HashEntry[]
        {
            new HashEntry(report.ExecId, value)
        });
    }
    public bool TryDequeueReport(out ReportFix report)
    {
        report = default!;
        if (OrderResponseQueue.TryDequeue(out ReportFix reportFound))
        {
            report = reportFound;
            return true;
        }
        return false;
    }
}
