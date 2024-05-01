using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
namespace MatchingX.Infra.Cache;
public class DropCopyCache : IDropCopyCache
{
    private readonly ConcurrentQueue<TradeCaptureReport> TradeCaptureQueue;
    private readonly ConcurrentQueue<ExecutionReport> ExecutionReportQueue;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbTradeCaptureReport;
    private readonly IDatabase _dbExecutionReport;
    private readonly ILogger<DropCopyCache> _logger;

    public DropCopyCache(ILogger<DropCopyCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        TradeCaptureQueue = new ConcurrentQueue<TradeCaptureReport>();
        ExecutionReportQueue = new ConcurrentQueue<ExecutionReport>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbTradeCaptureReport = _redis.GetDatabase((int)RedisDataBases.MatchingExecutionReport);
        _dbExecutionReport = _redis.GetDatabase((int)RedisDataBases.MatchingExecutedTrade);

        _logger = logger;
    }

    public async void AddExecutionReport(ExecutionReport execution)
    {
        ExecutionReportQueue.Enqueue(execution);
        await SetValueExecutionReportRedisAsync(execution);
    }

    public async void AddTradeCaptureReport(TradeCaptureReport trade)
    {
        TradeCaptureQueue.Enqueue(trade);
        await SetValueTradeCaptureReportRedisAsync(trade);
    }

    private async Task SetValueTradeCaptureReportRedisAsync(TradeCaptureReport report)
    {
        RedisKey key = new RedisKey(report.TradeId.ToString());
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(report));
        await _dbTradeCaptureReport.SetAddAsync(key, value);
    }

    private async Task SetValueExecutionReportRedisAsync(ExecutionReport report)
    {
        RedisKey key = new RedisKey(report.OrderID.ToString());
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(report));
        await _dbExecutionReport.SetAddAsync(key, value);
    }

    public bool TryDequeueExecuteReport(ExecutionReport execution)
    {
        if (ExecutionReportQueue.TryDequeue(out ExecutionReport executionFound))
        {
            execution = executionFound;
            return true;
        }
        return false;
    }

    public bool TryDequeueTradeCaptureReport(TradeCaptureReport trade)
    {
        if (TradeCaptureQueue.TryDequeue(out TradeCaptureReport tradeFound))
        {
            trade = tradeFound;
            return true;
        }
        return false;
    }
}