using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Enums;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OrderEngineX.Infra.Cache;
public class ExecutionReportCache : IExecutionReportCache
{
    private readonly ConnectionRedis _config;
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisKey _key = new RedisKey("ExecutionReportOrder");
    private readonly IDatabase _dbExecuitonReportCache;
    private readonly ILogger<ExecutionReportCache> _logger;

    public ExecutionReportCache(ILogger<ExecutionReportCache> logger, IOptions<ConnectionRedis> config)
    {
        
        _logger = logger;

        _config = config.Value;

        _redis = ConnectionMultiplexer.Connect(_config.ConnectionString, options => {
            options.ReconnectRetryPolicy = new ExponentialRetry(5000, 1000 * 60);
        });
        
        _dbExecuitonReportCache = _redis.GetDatabase((int)RedisDataBases.OrderEngine);
    }
    
    public async Task<Result> UpsertExecutionReportAsync(ExecutionReport execution)
    {
        RedisValue value = new RedisValue(JsonSerializer.Serialize<ExecutionReport>(execution));
        
        var key = string.Concat(_key, ":", execution.Symbol);
        await _dbExecuitonReportCache.HashSetAsync(key,
            new HashEntry[]
            {
                new HashEntry( execution.ExecID, value)
            });
        return Result.Ok();
    }

    public async Task<Result> DeleteExecutionReportAsync(ExecutionReport execution)
    {
        RedisValue value = new RedisValue(execution.ExecID.ToString());

        var key = string.Concat(_key, ":", execution.Symbol);
        var result = await _dbExecuitonReportCache.HashDeleteAsync(key, value);
        
        return result?Result.Ok(): Result.Fail("not found");
    }

    public async Task<Result<Dictionary<long, ExecutionReport>>> GetExecutionReportAsync(string symbol)
    {
        var result = new Dictionary<long, ExecutionReport>();
        var key = string.Concat(_key, ":", symbol);

        var hashEntry = await _dbExecuitonReportCache.HashGetAllAsync(key);

        foreach (var item in hashEntry)
        {
            var value = JsonSerializer.Deserialize<ExecutionReport>(item.Value!);
            result.Add(long.Parse(item.Name!), value!);
        }

        return Result.Ok(result);
    }

}