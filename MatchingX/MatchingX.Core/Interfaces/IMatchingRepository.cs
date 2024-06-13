using FluentResults;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public interface IMatchingRepository
{
    Task<(OrderStatus, Dictionary<long, OrderEngine>)> MatchingLimitAsync(OrderEngine orderEngine, CancellationToken cancellation);
    Task<(OrderStatus, Dictionary<long, OrderEngine>)> MatchingMarketAsync(OrderEngine orderEngine, CancellationToken cancellation);
    Task<Result> UpsertOrderMatchingAsync(OrderEngine orderEngine, CancellationToken cancellation);
    Task<Result> RemoveOrdersMatchingAsync(List<long> IdOrders, CancellationToken cancellation);
}