using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace DropCopyX.Infra.Cache;
public class TradeCaptureReportCache : IExecutedTradeCache
{
    private readonly IOptions<ConnectionRedis> _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger<TradeCaptureReportCache> _logger;
    private readonly IDatabase _dbDropCopy;
    private static ConcurrentQueue<TradeCaptureReport> ExecutedTradeIncrementalQueue=null!;
    
    private RedisKey keyTradeId = new RedisKey(Constants.RedisKeyTradeId);
    private RedisKey keyExecutedTrade = new RedisKey(Constants.RedisExecutedTrade);

    public TradeCaptureReportCache(ILogger<TradeCaptureReportCache> logger, IOptions<ConnectionRedis> config)
    {
        _logger = logger;
        ExecutedTradeIncrementalQueue = new ConcurrentQueue<TradeCaptureReport>();

        _redis = ConnectionMultiplexer.Connect(_config.Value.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbDropCopy = _redis.GetDatabase((int)RedisDataBases.DropCopy);
    }
    public async void AddExecutionReport(TradeCaptureReport trade)
    {
        ExecutedTradeIncrementalQueue.Enqueue(trade);
        await SetValueRedis(trade);
    }
    private async Task SetValueRedis(TradeCaptureReport trade)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<TradeCaptureReport>(trade));
        await _dbDropCopy.HashSetAsync(keyExecutedTrade, new HashEntry[]
        {
            new HashEntry(trade.TradeId, value)
        });
    }

    public long GetLastTradeId()
    {
        var TradeId =  _dbDropCopy.HashGetAsync(keyTradeId, new RedisValue("TradeId") );

        var execId = _dbDropCopy.StringGet("TradeId", CommandFlags.None);
        if (!execId.HasValue)
            return 0;
        return (long)execId;
    }
    public async void SetLastTradeId(long tradeId)
    {
        RedisValue value = new RedisValue(tradeId.ToString());
        await _dbDropCopy.HashIncrementAsync(keyTradeId, value);
    }

    public bool TryDequeueuExecutionReport(out TradeCaptureReport executionReport)
    {
        executionReport = default(TradeCaptureReport)!;
        if(ExecutedTradeIncrementalQueue.TryDequeue(out TradeCaptureReport executionFound))
        {
            executionReport = executionFound;
            return true;
        }
        return false;
    }

    public async Task<Dictionary<long, TradeCaptureReport>> GetSnapShotTradeCaptureReport()
    {
        var result = new Dictionary<long, TradeCaptureReport>();
        var trades = await _dbDropCopy.HashGetAllAsync(keyExecutedTrade);

        foreach (var trade in trades)
            result.TryAdd(long.Parse(trade.Name!), JsonSerializer.Deserialize<TradeCaptureReport>(trade.Value));


        return result;
    }

}