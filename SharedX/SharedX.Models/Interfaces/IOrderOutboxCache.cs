using FluentResults;
using SharedX.Core.ValueObjects;
namespace SharedX.Core.Interfaces;
public interface IOrderOutboxCache<T> where T : class
{
    Task<bool> DeleteOutboxAsync(EnvelopeOutbox<T> envelope);
    Task<bool> UpsertOutboxAsync(EnvelopeOutbox<T> envelope);
    Task<Result<Dictionary<long, EnvelopeOutbox<T>>>> GetOutboxByActivityAsync(string activity);
}