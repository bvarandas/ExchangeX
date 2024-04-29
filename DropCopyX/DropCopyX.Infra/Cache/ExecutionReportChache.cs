using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Proto;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
namespace DropCopyX.Infra.Cache;
public class ExecutionReportChache : IExecutionReportChache
{
    private readonly IOptions<ConnectionRedis> _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbExecutionReport;
    private readonly ILogger<ExecutionReportChache> _logger;

    private static ConcurrentQueue<ExecutionReport> ExecutionReportQueue;
    public ExecutionReportChache(ILogger<ExecutionReportChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config;
        
        ExecutionReportQueue = new ConcurrentQueue<ExecutionReport>();

        _redis = ConnectionMultiplexer.Connect(_config.Value.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbExecutionReport = _redis.GetDatabase((int)RedisDataBases.MatchingExecutionReport);

        _logger = logger;
        
    }

    public async void AddExecutionReport(ExecutionReport report)
    {
        ExecutionReportQueue.Enqueue(report);
        await SetValueRedis(report);
    }

    private async Task SetValueRedis(ExecutionReport report)
    {
        RedisKey key = new RedisKey(report.OrderID.ToString());
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(report));
        await _dbExecutionReport.SetAddAsync(key, value);
    }

    public ExecutionReport TryDequeueExecutionReport()
    {
        if (ExecutionReportQueue.TryDequeue(out ExecutionReport report))
        {
            return report;
        }
        return default(ExecutionReport);
    }

}