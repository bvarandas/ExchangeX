using FluentResults;
using SharedX.Core.ValueObjects;
namespace SharedX.Core.Interfaces;
public interface IOutboxRepository<T> where T : class
{
    Task<Result> DeleteOutboxAsync(EnvelopeOutbox<T> envelope, CancellationToken cancellationToken);
    Task<Result> UpsertOutboxAsync(EnvelopeOutbox<T> envelope, CancellationToken cancellationToken);
    Task<Result<Dictionary<long, EnvelopeOutbox<T>>>> GetOutboxByActivityAsync(string activity);
}