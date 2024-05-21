using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public interface IMatchingRepository
{
    Task<(OrderStatus, Dictionary<long, OrderEngine>)> MatchingLimitAsync(OrderEngine orderEngine, CancellationToken cancellation);
    Task<(OrderStatus, Dictionary<long, OrderEngine>)> MatchingMarketAsync(OrderEngine orderEngine, CancellationToken cancellation);
    Task<bool> UpsertOrderMatchingAsync(OrderEngine orderEngine, CancellationToken cancellation);
    Task<bool> RemoveOrdersMatchingAsync(List<long> IdOrders, CancellationToken cancellation);
}