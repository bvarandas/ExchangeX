using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core;
using SharedX.Core.Enums;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Text.Json;

namespace DropCopyX.Infra.Cache;
public class FixSessionCache : IFixSessionCache
{
    private readonly IOptions<ConnectionRedis> _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger<FixSessionCache> _logger;
    private readonly IDatabase _dbDropCopy;

    private readonly RedisKey _keyFixSession;
    public FixSessionCache(ILogger<FixSessionCache> logger, IOptions<ConnectionRedis> config)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect(_config.Value.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbDropCopy = _redis.GetDatabase((int)RedisDataBases.DropCopy);

        _keyFixSession = new RedisKey(Constants.RedisDropCopyFixSession);
    }

}