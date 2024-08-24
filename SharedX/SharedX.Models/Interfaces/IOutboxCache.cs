using FluentResults;
using SharedX.Core.ValueObjects;
namespace SharedX.Core.Interfaces;
public interface IOutboxCache<T> where T : class
{
    Task<Result> DeleteOutboxAsync(EnvelopeOutbox<T> envelope);
    Task<Result> UpsertOutboxAsync(EnvelopeOutbox<T> envelope);
    Task<Result<Dictionary<long, EnvelopeOutbox<T>>>> GetOutboxByActivityAsync(string activity);
    Task<Result<EnvelopeOutbox<T>>> TryDequeueZeroMQEnvelope(out EnvelopeOutbox<T> envelope);
    Task<Result<EnvelopeOutbox<T>>> TryDequeueRabbitMQEnvelope(out EnvelopeOutbox<T> envelope);
    Task<Result> TryEnqueueZeroMQEnvelope(EnvelopeOutbox<T> envelope);
    Task<Result> TryEnqueueRabitMQEnvelope(EnvelopeOutbox<T> envelope);
}