using FluentResults;
using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MarketDataX.Infra.Cache;
public class SecurityCache : ISecurityCache
{
    private static ConcurrentQueue<Security> SecurityQueue=null!;
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _dbSecurity;
    private readonly ILogger<SecurityCache> _logger;
    private RedisKey _key = new RedisKey("Security");
    public SecurityCache(ILogger<SecurityCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;
        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });

        _dbSecurity = _redis.GetDatabase((int)RedisDataBases.Fix);
        _logger = logger;
    }

    private async Task SetValueRedis(Security security)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<Security>(security));

        var key = string.Concat(_key, ":", security.Symbol);

        await _dbSecurity.HashSetAsync(key, new HashEntry[]
        {
            new HashEntry(security.SecurityID, value)
        });
    }

    public async Task<Result<Security>> GetSecurity(string symbol, string securityId)
    {
        var key = string.Concat(_key, ":", symbol);
        RedisValue value = new RedisValue(securityId);
        var securityHash = await _dbSecurity.HashGetAsync(key, value);
        if (securityHash.HasValue)
        {
            var security = JsonSerializer.Deserialize<Security>(securityHash);
            return Result.Ok(security!);
        }

        return Result.Fail(new Error($"Symbol {symbol} not found "));
    }
    public async Task<Result<bool>> UpsertSecurity(Security security)
    {
        SecurityQueue.Enqueue(security);
        await SetValueRedis(security);

        return Result.Ok(true);
    }
    public async Task<Result<bool>> RemoveSecurity(Security security)
    {
        var key = string.Concat(_key, ":",  security.Symbol);
        RedisValue value = new RedisValue(security.SecurityID);
        var securityHash = await _dbSecurity.HashDeleteAsync(key, value);
        return Result.Ok(true);
    }
}