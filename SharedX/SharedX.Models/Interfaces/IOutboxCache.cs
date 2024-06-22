using FluentResults;
using SharedX.Core.ValueObjects;
namespace SharedX.Core.Interfaces;
public interface IOutboxCache<T> where T : class
{
    Task<Result> DeleteOutboxAsync(EnvelopeOutbox<T> envelope);
    Task<Result> UpsertOutboxAsync(EnvelopeOutbox<T> envelope);
    Task<Result<Dictionary<long, EnvelopeOutbox<T>>>> GetOutboxByActivityAsync(string activity);
}