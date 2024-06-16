using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OrderEngineX.Infra.Cache;

public class SecurityEngineCache : ISecurityEngineCache
{
    private static ConcurrentDictionary<string, SecurityEngine> DictionarySecurityEngine = null!;
    private readonly ConnectionRedis _config;
    private IDatabase _dbSecurity;
    private readonly ILogger<SecurityEngineCache> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _key;

    public SecurityEngineCache(ILogger<SecurityEngineCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        DictionarySecurityEngine = new ConcurrentDictionary<string, SecurityEngine>();

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbSecurity = _redis.GetDatabase((int)RedisDataBases.Security);
        _logger = logger;
        _key = new RedisKey("Security");
    }

    public bool TryGetSecurity(string symbol, out SecurityEngine security)
    {
        security = default(SecurityEngine)!;
        if ( DictionarySecurityEngine.TryGetValue(symbol, out SecurityEngine securityFound))
        {
            security = securityFound;
            return true;
        }else
        {
            var result = GetSecurityBySymbolAsync(symbol);
            if (result.Result.IsFailed)
                return false;

            security = result.Result.Value;
            return true;
        }

    }

    private async Task<Result<SecurityEngine>> GetSecurityBySymbolAsync(string symbol)
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
}
