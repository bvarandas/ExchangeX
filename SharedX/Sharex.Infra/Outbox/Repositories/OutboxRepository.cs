using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Sharedx.Infra.Outbox.Data;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.ValueObjects;

namespace Sharedx.Infra.Outbox.Repositories;
public class OutboxRepository<T> where T : class , IOutboxRepository<T>
{
    private readonly ILogger<OutboxRepository<T>> _logger = null!;
    private readonly IOutboxContext<T> _context = null!;
    public OutboxRepository(ILogger<OutboxRepository<T>> logger,
        IOutboxContext<T> context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> UpsertOutboxAsync(EnvelopeOutbox<T> envelope, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<EnvelopeOutbox<T>>.Filter;
            var filter = builder.Eq(o => o.Id, envelope.Id);

            var resultReplace = await _context.Collection.ReplaceOneAsync(filter,
                replacement: envelope,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken);

            result = resultReplace.IsAcknowledged && resultReplace.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        return Result.Ok();
    }

    public async Task<Result> DeleteOutboxAsync(EnvelopeOutbox<T> envelope, CancellationToken cancellationToken)
    {
        bool result = false;
        try
        {
            var builder = Builders<EnvelopeOutbox<T>>.Filter;
            var filter = builder.Eq(o => o.Id, envelope.Id);

            var resultReplace = await _context.Collection.DeleteManyAsync(filter,
                options: new DeleteOptions{ },
                cancellationToken);

            result = resultReplace.IsAcknowledged && resultReplace.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Result.Fail(new Error(ex.Message));
        }
        if (result)
            return Result.Ok();
        else
            return Result.Fail(new Error("Não deletou nenhum registro."));
    }
}
