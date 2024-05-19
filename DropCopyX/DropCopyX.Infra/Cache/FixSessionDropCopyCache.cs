using DropCopyX.Core.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using SharedX.Core;
using SharedX.Core.Enums;
using SharedX.Core.Matching;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.ServiceModel.Channels;
using System.Text.Json;

namespace DropCopyX.Infra.Cache;
public class FixSessionDropCopyCache : IFixSessionDropCopyCache
{
    private readonly IOptions<ConnectionRedis> _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger<FixSessionDropCopyCache> _logger;
    private readonly IDatabase _dbFix;

    private readonly RedisKey _keyFixSession;
    public FixSessionDropCopyCache(ILogger<FixSessionDropCopyCache> logger, IOptions<ConnectionRedis> config)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect(_config.Value.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbFix = _redis.GetDatabase((int)RedisDataBases.Fix);

        _keyFixSession = new RedisKey(KeyNameRedis.DropCopyFixSession);
    }

    public async void AddSessionAsync(QuickFix.FIX44.Message request, SessionID sessionID)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<QuickFix.FIX44.Message>(request));
        var key = string.Concat(_keyFixSession, ":", sessionID);
        await _dbFix.HashSetAsync(key, new HashEntry[]
        {
            new HashEntry(request.Header.GetString(Tags.MsgType), value)
        });
    }

    public async Task<bool> RemoveSessionAsync(QuickFix.FIX44.Message request, SessionID sessionID)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<QuickFix.FIX44.Message>(request));
        var key = string.Concat(_keyFixSession, ":", sessionID);

        var result = await _dbFix.HashDeleteAsync(key, value);
        return result;
    }
}