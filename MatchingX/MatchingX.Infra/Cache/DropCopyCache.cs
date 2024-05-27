using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MatchingX.Infra.Cache;
public class DropCopyCache : IDropCopyCache
{
    private readonly ConcurrentQueue<TradeCaptureReport> TradeCaptureQueue;
    private readonly ConcurrentQueue<ExecutionReport> ExecutionReportQueue;
    private readonly ConcurrentQueue<ExecutionReport> ExecutionReportToOrderQueue;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMatching;

    private RedisKey keyTradeCapture = new RedisKey("TradeCapture");
    private RedisKey keyExecuteReport = new RedisKey("ExecuteReport");

    private readonly ILogger<DropCopyCache> _logger;

    public DropCopyCache(ILogger<DropCopyCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        TradeCaptureQueue = new ConcurrentQueue<TradeCaptureReport>();
        ExecutionReportQueue = new ConcurrentQueue<ExecutionReport>();
        ExecutionReportToOrderQueue = new ConcurrentQueue<ExecutionReport>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    public async void AddExecutionReport(ExecutionReport execution)
    {
        ExecutionReportQueue.Enqueue(execution);
        ExecutionReportToOrderQueue.Enqueue(execution);
        await SetValueExecutionReportRedisAsync(execution);
    }

    public async void AddTradeCaptureReport(TradeCaptureReport trade)
    {
        TradeCaptureQueue.Enqueue(trade);
        await SetValueTradeCaptureReportRedisAsync(trade);
    }

    private async Task SetValueTradeCaptureReportRedisAsync(TradeCaptureReport report)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<TradeCaptureReport>(report));
        _dbMatching.HashIncrement(keyTradeCapture, value);
    }

    private async Task SetValueExecutionReportRedisAsync(ExecutionReport report)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<ExecutionReport>(report));
        _dbMatching.HashIncrement(keyExecuteReport, value);
    }

    public bool TryDequeueExecuteReport(out ExecutionReport execution)
    {
        execution = default(ExecutionReport)!;
        if (ExecutionReportQueue.TryDequeue(out ExecutionReport executionFound))
        {
            execution = executionFound;
            return true;
        }
        return false;
    }

    public bool TryDequeueExecuteToOrderReport(out ExecutionReport execution)
    {
        execution = default(ExecutionReport)!;
        if (ExecutionReportToOrderQueue.TryDequeue(out ExecutionReport executionFound))
        {
            execution = executionFound;
            return true;
        }
        return false;
    }

    public bool TryDequeueTradeCaptureReport(out TradeCaptureReport trade)
    {
        trade = default(TradeCaptureReport)!;
        if (TradeCaptureQueue.TryDequeue(out TradeCaptureReport tradeFound))
        {
            trade = tradeFound;
            return true;
        }
        return false;
    }
}