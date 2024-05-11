using FluentResults;
using SharedX.Core.Matching.OrderEngine;
namespace SharedX.Core.Interfaces;
public interface IOrderOutboxCache
{
    Task<bool> DeleteOutboxAsync(string activity, long orderId);
    Task<bool> UpsertOutboxAsync(OrderEngine order, string activity);
    Task<Result<Dictionary<long, OrderEngine>>> GetOutboxAsync(string activity);
}