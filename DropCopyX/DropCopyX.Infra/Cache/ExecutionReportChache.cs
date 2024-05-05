using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
namespace DropCopyX.Infra.Cache;
public class ExecutionReportChache : IExecutionReportChache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbExecutionReport;
    private readonly ILogger<ExecutionReportChache> _logger;
    private static ConcurrentQueue<ExecutionReport> ExecutionReportQueue;
    private RedisKey keyExecutionReport = new RedisKey(Constants.RedisExecutionReport);
    public ExecutionReportChache(ILogger<ExecutionReportChache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        ExecutionReportQueue = new ConcurrentQueue<ExecutionReport>();
        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbExecutionReport = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }
    public async void AddExecutionReport(ExecutionReport report)
    {
        ExecutionReportQueue.Enqueue(report);
        await SetValueRedis(report);
    }
    private async Task SetValueRedis(ExecutionReport report)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(report));
        await _dbExecutionReport.HashIncrementAsync(keyExecutionReport, value);
    }
    public bool TryDequeueExecutionReport(out ExecutionReport report)
    {
        report = default(ExecutionReport);
        if (ExecutionReportQueue.TryDequeue(out ExecutionReport reportFound))
        {
            report = reportFound;
            return true;
        }
        return false;
    }
}