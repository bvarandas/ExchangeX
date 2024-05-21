using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickFix.Config;
using SharedX.Core.Enums;
using SharedX.Core.Specs;
using StackExchange.Redis;

namespace MatchingX.Infra.Cache;
public class SecurityCache : ISecurityCache
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbSecurity;
    private readonly ILogger<MatchingCache> _logger;
    private readonly ConnectionMultiplexer _redis;

    private RedisKey keySecurity = new RedisKey("Security");
    public SecurityCache(ILogger<MatchingCache> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbSecurity = _redis.GetDatabase((int)RedisDataBases.Matching);
        _logger = logger;
    }


}
