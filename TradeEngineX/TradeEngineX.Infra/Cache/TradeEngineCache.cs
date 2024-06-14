using Microsoft.Extensions.Logging;
using TradeEngineX.Core.Interfaces;
using StackExchange.Redis;
using System.Collections.Concurrent;
using SharedX.Core.Specs;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using System.Text.Json;
using FluentResults;
namespace TradeEngineX.Infra.Cache;
public class TradeEngineCache : ITradeEngineCache
{
    private static ConcurrentQueue<TradeReport> TradeEngineQueue = null!;
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbTradeEngine;
    private readonly ILogger<TradeEngineCache> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _key;

    public TradeEngineCache(ILogger<TradeEngineCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        TradeEngineQueue = new ConcurrentQueue<TradeReport>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbTradeEngine = _redis.GetDatabase((int)RedisDataBases.Trade);
        _logger = logger;
        _key = new RedisKey("TradeEngine");
    }

    public async Task<Result<TradeReport>> GetTradeByIdAsync(long tradeId)
    {
        var result = new TradeReport();
        var key = string.Concat(_key,":", tradeId);
        RedisValue value = new RedisValue(key);

        var hashEntry = await _dbTradeEngine.HashGetAsync(key, value);

        if (hashEntry.HasValue)
            return Result.Fail(new Error($"Trade {tradeId} not found"));

        result = JsonSerializer.Deserialize<TradeReport>(hashEntry!);
        return Result.Ok(result!);
    }

    public async Task UpsertTradeEngineAsync(TradeReport trade)
    {
        TradeEngineQueue.Enqueue(trade);
        RedisValue value = new RedisValue(JsonSerializer.Serialize<TradeReport>(trade));
        var key = string.Concat(_key, ":", trade.TradeId);

        await _dbTradeEngine.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(trade.TradeId, value)
            });
    }

    public async Task RemoveTradeEngineAsync(TradeReport trade)
    {
        TradeEngineQueue.Enqueue(trade);
        RedisValue value = new RedisValue(trade.TradeId.ToString());
        var key = string.Concat(_key, ":", trade.TradeId);

        await _dbTradeEngine.HashDeleteAsync(key, value);
    }

    public bool TryDequeueTradeEngine(out TradeReport trade)
    {
        trade = default(TradeReport)!;
        if (TradeEngineQueue.TryDequeue(out TradeReport tradeFound))
        {
            trade = tradeFound;
            return true;
        }
        return false;
    }
}