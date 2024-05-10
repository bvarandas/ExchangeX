using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NRedisStack.Graph;
using SharedX.Core;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace DropCopyX.Infra.Cache;
public class ExecutedTradeCache : IExecutedTradeCache
{
    private readonly IOptions<ConnectionRedis> _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger<ExecutedTradeCache> _logger;
    private readonly IDatabase _dbMatching;
    private static ConcurrentQueue<TradeCaptureReport> ExecutedTradeQueue;
    
    private RedisKey keyTradeId = new RedisKey(Constants.RedisKeyTradeId);
    private RedisKey keyExecutedTrade = new RedisKey(Constants.RedisExecutedTrade);

    public ExecutedTradeCache(ILogger<ExecutedTradeCache> logger, IOptions<ConnectionRedis> config)
    {
        _logger = logger;
        ExecutedTradeQueue = new ConcurrentQueue<TradeCaptureReport>();

        _redis = ConnectionMultiplexer.Connect(_config.Value.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
    }
    public async void AddExecutionReport(TradeCaptureReport trade)
    {
        ExecutedTradeQueue.Enqueue(trade);
        await SetValueRedis(trade);
    }
    private async Task SetValueRedis(TradeCaptureReport trade)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<TradeCaptureReport>(trade));
        await _dbMatching.HashSetAsync(keyExecutedTrade, new HashEntry[]
        {
            new HashEntry(trade.TradeId, value)
        });
    }

    public long GetLastTradeId()
    {
        var TradeId =  _dbMatching.HashGetAsync(keyTradeId, new RedisValue("TradeId") );

        var execId = _dbMatching.StringGet("TradeId", CommandFlags.None);
        if (!execId.HasValue)
            return 0;
        return (long)execId;
    }
    public async void SetLastTradeId(long tradeId)
    {
        RedisValue value = new RedisValue(tradeId.ToString());
        await _dbMatching.HashIncrementAsync(keyTradeId, value);
    }
}