using Microsoft.Extensions.Logging;
using SecurityX.Core.Interfaces;
using SharedX.Core.Specs;
using FluentResults;
using StackExchange.Redis;
using SharedX.Core.Enums;
using Microsoft.Extensions.Options;
using SharedX.Core.Matching.OrderEngine;
using System.Text.Json;
using SharedX.Core.Entities;

namespace SecurityX.Infra.Cache;

public class SecurityCache: ISecurityCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbSecurity;
    private readonly ILogger<SecurityCache> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _key;

    public SecurityCache(ILogger<SecurityCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbSecurity = _redis.GetDatabase((int)RedisDataBases.Security);
        _logger = logger;
        _key = new RedisKey("Security");
    }

    public async Task<Result<SecurityEngine>> GetSecurityBySymbolAsync( string symbol)
    {
        var result = new SecurityEngine();
        var key = string.Concat(_key);
        RedisValue value = new RedisValue(symbol);
        var hashEntry = await _dbSecurity.HashGetAsync(key, value);

        if (hashEntry.HasValue)
            return Result.Fail(new Error($"Security {symbol} not found"));

        result = JsonSerializer.Deserialize<SecurityEngine>(hashEntry!);
        return Result.Ok(result!);
    }

    public async Task<Result<Dictionary<string, SecurityEngine>>> GetAllSecurityAsync()
    {
        var result = new Dictionary<string, SecurityEngine>();
        var key = string.Concat(_key);
        var hashEntry = await _dbSecurity.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<SecurityEngine>(item.Value);
            result.Add(item.Name, value);
        }
        return Result.Ok(result);
    }

    public async Task UpsertSecurityAsync(SecurityEngine security)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<SecurityEngine>(security));
        var key = string.Concat(_key, ":", security.Symbol);
        await _dbSecurity.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(security.Symbol, value)
            });
    }
}