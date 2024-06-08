using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MatchingX.Infra.Cache;
public class MatchingCache :IMatchingCache
{
    private readonly ConcurrentQueue<TradeCaptureReport> TradeCaptureQueue;
    private readonly ConcurrentQueue<ExecutionReport> ExecutionReportQueue;
    private readonly ConcurrentQueue<ExecutionReport> ExecutionReportToOrderQueue;
    private readonly ConcurrentQueue<MarketData> IncrementalQueue = null!;
    private readonly ConcurrentDictionary<string, decimal> _LastPrice;

    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMatching;
    private readonly ILogger<MatchingCache> _logger;
    
    private static long MarketID = 0;

    private RedisKey keyMarketData = new RedisKey("Marketdata");
    private RedisKey keyTradeCapture = new RedisKey("TradeCapture");
    private RedisKey keyExecuteReport = new RedisKey("ExecuteReport");

    public MatchingCache(ILogger<MatchingCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        IncrementalQueue = new ConcurrentQueue<MarketData>();
        _LastPrice = new ConcurrentDictionary<string, decimal>();
        TradeCaptureQueue = new ConcurrentQueue<TradeCaptureReport>();
        ExecutionReportQueue = new ConcurrentQueue<ExecutionReport>();
        ExecutionReportToOrderQueue = new ConcurrentQueue<ExecutionReport>();


        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    #region DropCopy

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
    #endregion

    #region Marketdata
    public async Task<decimal> GetPrice(string symbol)
    {
        RedisValue value = new RedisValue(symbol);
        var marketHash = await _dbMatching.HashGetAsync(keyMarketData, value);
        if (marketHash.HasValue)
        {
            var marketData = JsonSerializer.Deserialize<MarketData>(marketHash);
            return marketData!.EntryPx;
        }
        return 0;
    }
    public async Task<MarketData> GetMarketDataBySymbol(string symbol)
    {
        var marketData = new MarketData();
        RedisValue value = new RedisValue(symbol);
        var marketHash = await _dbMatching.HashGetAsync(keyMarketData, value);
        if (marketHash.HasValue)
        {
            marketData = JsonSerializer.Deserialize<MarketData>(marketHash);

        }
        return default(MarketData)!;
    }
    private async Task SetValueRedis(MarketData marketData)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MarketData>(marketData));

        await _dbMatching.HashSetAsync(keyMarketData, new HashEntry[]
        {
            new HashEntry(marketData.Symbol, value)
        });
    }

    public bool TryDequeueMarketData(out MarketData marketData)
    {
        marketData = default(MarketData);
        if (IncrementalQueue.TryDequeue(out MarketData marketDataFound))
        {
            marketData = marketDataFound;
            return true;
        }
        return false;
    }

    public async void AddIncremental(MarketData marketData)
    {
        marketData.Id = ++MarketID;
        IncrementalQueue.Enqueue(marketData);
        await SetValueRedis(marketData);
    }
    #endregion
}