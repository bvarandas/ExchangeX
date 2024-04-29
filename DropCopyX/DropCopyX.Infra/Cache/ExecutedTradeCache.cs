using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Proto;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
namespace DropCopyX.Infra.Cache;
public class ExecutedTradeCache : IExecutedTradeCache
{
    private readonly IOptions<ConnectionRedis> _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger<ExecutedTradeCache> _logger;
    private readonly IDatabase _dbExecutedTrade;
    private readonly IDatabase _dbTradeId;
    private static ConcurrentQueue<ExecutedTrade> ExecutedTradeQueue;
    public ExecutedTradeCache(ILogger<ExecutedTradeCache> logger, IOptions<ConnectionRedis> config)
    {
        _logger = logger;
        ExecutedTradeQueue = new ConcurrentQueue<ExecutedTrade>();

        _redis = ConnectionMultiplexer.Connect(_config.Value.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbExecutedTrade = _redis.GetDatabase((int)RedisDataBases.MatchingExecutedTrade);
        _dbTradeId = _redis.GetDatabase((int)RedisDataBases.MatchingTradeId);
    }
    public async void AddExecutionReport(ExecutedTrade trade)
    {
        ExecutedTradeQueue.Enqueue(trade);
        await SetValueRedis(trade);
    }
    private async Task SetValueRedis(ExecutedTrade trade)
    {
        RedisKey key = new RedisKey(trade.TradeId.ToString());
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(trade));
        await _dbExecutedTrade.SetAddAsync(key, value);
    }

    public long GetLastTradeId()
    {
        var execId =  _dbTradeId.StringGet("TradeId", CommandFlags.None);
        if (!execId.HasValue)
            return 0;
        return (long)execId;
    }
    public async void SerLastTradeId(long tradeId)
    {
        RedisKey key = new RedisKey("TradeId");
        RedisValue value = new RedisValue(tradeId.ToString());
        await _dbTradeId.SetAddAsync(key, value);
    }
}