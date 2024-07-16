using FluentResults;
using MassTransit.Middleware.InMemoryOutbox;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Matching;
using SharedX.Core.ValueObjects;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading;

namespace Sharedx.Infra.Outbox.Data;
public class OutboxContext<T> where T:class, IOutboxContext<T>
{
    private readonly IOutboxContext<T> _context;
    private readonly ILogger<OutboxContext<T>> _logger;
    public OutboxContext(ILogger<OutboxContext<T>> logger, IOutboxContext<T> context) 
    { 
        _context = context;
        _logger = logger;
    }

    public async Task<Result> DeleteOutboxAsync(EnvelopeOutbox<T> envelope, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<EnvelopeOutbox<T>>.Filter;
            var filter = builder.Eq(o => o.Id, envelope.Id);

            var resultReplace = await _context.Collection.DeleteOneAsync(filter,
                options: new DeleteOptions { },
                cancellationToken);
            result = (resultReplace.IsAcknowledged && resultReplace.DeletedCount> 0);

            if (!result)
                return Result.Fail(new Error("Não inseriu o envelope no banco"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }

    public async Task<Result<Dictionary<long, EnvelopeOutbox<T>>>> GetOutboxByActivityAsync(string activity)
    {
        var result = new Dictionary<long, EnvelopeOutbox<T>>();
        //var key = string.Concat(_key, ":", activity);

        //var hashEntry = await _dbOutboxCache.HashGetAllAsync(key);

        //foreach (var item in hashEntry)
        //{
        //    var value = JsonSerializer.Deserialize<EnvelopeOutbox<T>>(item.Value!);
        //    result.Add(long.Parse(item.Name!), value!);
        //}

        return Result.Ok(result);
    }

    public async Task<Result> UpsertOutboxAsync(EnvelopeOutbox<T> envelope, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<EnvelopeOutbox<T>>.Filter;
            var filter = builder.Eq(o => o.Id, envelope.Id);
            using (var session = await _context.MongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                var resultReplace = await _context.Collection.ReplaceOneAsync(session,filter,
                replacement: envelope,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken);
                result = (resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0);

                if (!result)
                    return Result.Fail(new Error("Não inseriu o envelope no banco"));
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }
}
