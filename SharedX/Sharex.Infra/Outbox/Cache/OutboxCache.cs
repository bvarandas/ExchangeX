using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Sharedx.Infra.Outbox.Cache;
public class OutboxCache<T>  where T : class,  IOutboxCache<T>
{
    private readonly ConnectionRedis _config;
    private readonly IDatabase _dbOutboxCache;
    private readonly ILogger<OutboxCache<T>> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _key = new RedisKey("Outbox");

    private readonly ConcurrentQueue<T> _queueZeroMQ = null!;
    private readonly ConcurrentQueue<T> _queueRabbitMQ= null!;

    public OutboxCache(ILogger<OutboxCache<T>> logger, IOptions<ConnectionRedis> config)
    {
        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        _dbOutboxCache = _redis.GetDatabase((int)RedisDataBases.Outbox);
        _logger = logger;
        _key = new RedisKey("Outbox");
    }

    public async Task<bool> DeleteOutboxAsync(EnvelopeOutbox<T> envelope)
    {
        RedisValue value = new RedisValue(envelope.Id.ToString());
        var key = string.Concat(_key, ":", envelope.ActivityOutbox.Activity);
        var result = await _dbOutboxCache.HashDeleteAsync(key, value);
        return result;
    }

    public async Task<Result<Dictionary<long, EnvelopeOutbox<T>>>> GetOutboxByActivityAsync(string activity)
    {
        var result = new Dictionary<long, EnvelopeOutbox<T>>();
        var key = string.Concat(_key,  ":", activity );

        var hashEntry = await _dbOutboxCache.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<EnvelopeOutbox<T>>(item.Value!);
            result.Add(long.Parse(item.Name!), value!);
        }

        return Result.Ok(result);
    }

    public async Task<bool> UpsertOutboxAsync(EnvelopeOutbox<T> envelope)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<EnvelopeOutbox<T>>(envelope));
        var key = string.Concat(_key, ":", envelope.ActivityOutbox.Activity);
        await _dbOutboxCache.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry(envelope.Id, value)
            });
        return true;
    }

    public async Task<Result> TryDequeueZeroMQEnvelope(out EnvelopeOutbox<T> envelope)
    {
        
    }

    public async Task<Result> TryDequeueRabbitMQEnvelope(out EnvelopeOutbox<T> envelope)
    {

    }

}