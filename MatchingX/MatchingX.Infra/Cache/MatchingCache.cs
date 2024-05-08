using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using StackExchange.Redis;
using FluentResults;
using System.Collections.Concurrent;
using MongoDB.Driver.Linq;
namespace MatchingX.Infra.Cache;
public class MatchingCache : IMatchingCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbMatching;
    private readonly ILogger<MatchingCache> _logger;
    private readonly ConnectionMultiplexer _redis;

    private RedisKey keyBuyOrders = new RedisKey("BuyOrders");
    private RedisKey keySellOrders = new RedisKey("SellOrders");

    protected readonly ConcurrentDictionary<string, Dictionary<long, OrderEngine>> _buyOrders;
    protected readonly ConcurrentDictionary<string, Dictionary<long, OrderEngine>> _sellOrders;

    public MatchingCache(ILogger<MatchingCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbMatching = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;

        _buyOrders = new ConcurrentDictionary<string, Dictionary<long, OrderEngine>>();
        _sellOrders = new ConcurrentDictionary<string, Dictionary<long, OrderEngine>>();
    }
    public async Task<Result<OrderEngine>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol)
    {
        var result = new OrderEngine();
        var key = string.Concat(keyBuyOrders, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);

        var order = hashEntry.GetValue(orderId);
        
        if ( order is null)
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));
        
        result = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderEngine>(order?.ToString());
        
        return Result.Ok(result);
    }

    public async Task<Result<OrderEngine>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol)
    {
        var result = new OrderEngine();
        var key = string.Concat(keySellOrders, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);

        var order = hashEntry.GetValue(orderId);

        if (order is null)
            return Result.Fail(new Error($"Order {orderId} with symbol {symbol} not found"));

        result = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderEngine>(order?.ToString());
        
        return Result.Ok(result);
    }

    public async Task<Result<Dictionary<long, OrderEngine>>> GetBuyOrderBySymbol(string symbol)
    {
        var result =new Dictionary<long, OrderEngine>();
        var key = string.Concat(keyBuyOrders, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);
        foreach (var item in hashEntry)
        {
            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderEngine>(item.Value);
            result.Add(long.Parse(item.Name), value);
        }
        return Result.Ok(result);
    }

    public async Task<Result<Dictionary<long, OrderEngine>>> GetSellOrderBySymbol(string symbol)
    {
        var result = new Dictionary<long, OrderEngine>();
        var key = string.Concat(keyBuyOrders, ":", symbol);
        var hashEntry = await _dbMatching.HashGetAllAsync(key);
        foreach (var item in hashEntry)
        {
            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderEngine>(item.Value);
            result.Add(long.Parse(item.Name), value);
        }
        return Result.Ok(result);
    }

    public async void AddBuyOrder(OrderEngine order)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(order));
        var key = string.Concat(keyBuyOrders, ":", order.Symbol);
        await _dbMatching.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
        this.SetDictionaryOrderBuyCache(order);
    }

    public async void UpdateBuyOrder(OrderEngine order)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(order));
        var key = string.Concat(keyBuyOrders, ":", order.Symbol);
        await _dbMatching.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
        this.SetDictionaryOrderBuyCache(order);
    }
    public async void AddSellOrder(OrderEngine order)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(order));
        var key = string.Concat(keySellOrders, ":", order.Symbol);
        await _dbMatching.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
        this.SetDictionaryOrderSellCache(order);
    }

    public async void UpdateSellOrder(OrderEngine order)
    {
        RedisValue value = new RedisValue(Newtonsoft.Json.JsonConvert.SerializeObject(order));
        var key = string.Concat(keySellOrders, ":", order.Symbol);
        await _dbMatching.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(order.OrderID, value)
            });
        this.SetDictionaryOrderSellCache(order);
    }

    public bool TryGetBuyOrders(string symbol, out Dictionary<long, OrderEngine> dic)
    {
        dic = default(Dictionary<long, OrderEngine>)!;
        if (_buyOrders.TryGetValue(symbol, out Dictionary<long,OrderEngine> orderEngineFound))
        {
            dic = orderEngineFound;
            return true;
        }
        return false;
    }

    public bool TryGetSellOrders(string symbol, out Dictionary<long, OrderEngine> dic)
    {
        dic = default(Dictionary<long, OrderEngine>)!;
        if (_sellOrders.TryGetValue(symbol, out Dictionary<long, OrderEngine> orderEngineFound))
        {
            dic = orderEngineFound;
            return true;
        }
        return false;
    }

    private void SetDictionaryOrderBuyCache(OrderEngine order)
    {
        if (_buyOrders.TryGetValue(order.Symbol, out var dicOut))
        {
            dicOut.TryAdd(order.OrderID, order);
        }
        else
        {
            dicOut = new Dictionary<long, OrderEngine>();
            dicOut.Add(order.OrderID, order);
        }
        _buyOrders.AddOrUpdate(order.Symbol, dicOut, (key, oldValue) => oldValue);
    }

    private void SetDictionaryOrderSellCache(OrderEngine order)
    {
        if (_sellOrders.TryGetValue(order.Symbol, out var dicOut))
        {
            dicOut.TryAdd(order.OrderID, order);
        }
        else
        {
            dicOut = new Dictionary<long, OrderEngine>();
            dicOut.Add(order.OrderID, order);
        }
        _sellOrders.AddOrUpdate(order.Symbol, dicOut, (key, oldValue) => oldValue);
    }

    
}