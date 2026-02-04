using FluentResults;
using MatchingX.Core.Entities;
using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchingX.Infra.Cache;

public class BookOfferCache : IBookOfferCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbBook;
    private readonly ILogger<BookOfferCache> _logger;

    private RedisKey keyBuy = new RedisKey("book_offer_buy");
    private RedisKey keySell = new RedisKey("book_offer_sell");

    private RedisKey keyPrice = new RedisKey("last_price");

    private readonly ConnectionMultiplexer _redis;
    public BookOfferCache(ILogger<BookOfferCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options =>
        {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbBook = _redis.GetDatabase((int)RedisDataBases.OfferBook);
        _logger = logger;
    }
    public async Task<Result<MatchOrder>> GetOrderByIdandSymbolAsync(long orderId, string symbol, SideTrade side)
    {
        var result = new MatchOrder();

        var key = side == SideTrade.Buy ? $"{keyBuy}:{symbol}" : $"{keySell}:{symbol}";

        RedisValue value = new RedisValue(orderId.ToString());
        var hashEntry = await _dbBook.HashGetAsync(key, value);

        if (hashEntry.HasValue)
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));

        result = JsonSerializer.Deserialize<MatchOrder>(hashEntry!);
        return Result.Ok(result);
    }


    public async Task<Result<Dictionary<long, MatchOrder>>> GetOrderBySymbolAsync(string symbol, SideTrade side)
    {
        var result = new Dictionary<long, MatchOrder>();
        var key = side == SideTrade.Buy ? $"{keyBuy}:{symbol}" : $"{keySell}:{symbol}";

        var hashEntry = await _dbBook.HashGetAllAsync(key);
        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<MatchOrder>(item.Value);
            result.Add(long.Parse(item.Name), value);
        }
        return Result.Ok(result);
    }

    public async Task<bool> UpsertOrder(MatchOrder order)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MatchOrder>(order));

        var key = order.Side == SideTrade.Buy ? $"{keyBuy}:{order.Symbol}" : $"{keySell}:{order.Symbol}";

        await _dbBook.HashSetAsync(key,
        new HashEntry[]
        {
            new HashEntry(order.OrderID, value)
            });
        return true;
    }
    public async Task<bool> DeleteOrderAsync(string symbol, long orderId, SideTrade side)
    {
        RedisValue value = new RedisValue(orderId.ToString());

        var key = string.Concat(side == SideTrade.Buy ? keyBuy : keySell, ":", symbol);

        var result = await _dbBook.HashDeleteAsync(key, value);
        return result;
    }

    public async Task<bool> DeleteAllOrderAsync(Dictionary<long, MatchOrder> dicOrders)
    {
        bool result = false;
        foreach (var order in dicOrders.Values)
        {
            RedisValue value = new RedisValue(order.OrderID.ToString());
            if (order.Side.Equals(SideTrade.Sell))
            {
                var key = string.Concat(keySell, ":", order.Symbol);
                result = await _dbBook.HashDeleteAsync(key, value);
            }
            else if (order.Side.Equals(SideTrade.Buy))
            {
                var key = string.Concat(keyBuy, ":", order.Symbol);
                result = await _dbBook.HashDeleteAsync(key, value);
            }
        }
        return result;
    }
    public async Task<decimal> GetPrice(string symbol)
    {
        RedisValue value = new RedisValue(symbol);
        var marketHash = await _dbBook.HashGetAsync(keyPrice, value);

        if (marketHash.HasValue)
        {
            var marketData = JsonSerializer.Deserialize<MarketData>(marketHash);
            return marketData!.EntryPx;
        }
        return 0;
    }
    public async Task<Result> UpsertPrice(MarketData marketData)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<MarketData>(marketData));
        var key = marketData.Symbol;

        await _dbBook.HashSetAsync(keyPrice,
            new HashEntry[]{
                new HashEntry(key, value)
            });

        return Result.Ok();

    }

}