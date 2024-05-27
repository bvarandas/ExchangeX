using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Text.Json;

namespace Sharedx.Infra.Order.Cache;
public class BookOfferCache : IBookOfferCache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbMatching;

    private RedisKey _keyBook = new RedisKey("book");
    private readonly ILogger<BookOfferCache> _logger;

    public BookOfferCache(ILogger<BookOfferCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }

    public async Task<Result<bool>> AddBookItemAsync(Book book)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<Book>(book));

        var key = string.Concat(_keyBook, ":", book.Symbol);

        await _dbMatching.HashSetAsync(key, new HashEntry[]
        {
            new HashEntry( book.OrderId.ToString(), value)
        });
        return Result.Ok(true);
    }

    public async Task<Result<bool>> RemoveBookItemAsync(Book book)
    {
        var key = string.Concat(_keyBook, ":", book.Symbol);
        RedisValue value = new RedisValue( book.OrderId.ToString());
        var bookHash = await _dbMatching.HashDeleteAsync(key, value);
        return Result.Ok(true);
    }

    public async Task<Result<OrderBook>> GetBookAsync(string symbol)
    {
        var result = new OrderBook();
        var key = string.Concat(_keyBook, ":", symbol);
        var bookHash = await _dbMatching.HashGetAllAsync(key);
        foreach (var book in bookHash)
        {
            var bookObject = JsonSerializer.Deserialize<Book>(book.Value!);
            result.AddOrder(bookObject!);
        }
        result.Symbol = symbol;
        result.Timestamp = DateTime.UtcNow;

        if (bookHash.Count() > 0)
            return Result.Ok(result!);
        
        return Result.Fail(new Error($"Symbol {symbol} not found "));
    }
}