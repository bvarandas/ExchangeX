using MatchingX.Core.Entities;

namespace MatchingX.Core.Interfaces;
public interface IMatch
{
    string Name { get; }
    Task<bool> ReceiveOrderAsync(MatchOrder order, CancellationToken cancellationToken);
    Task<bool> ModifyOrderAsync(MatchOrder order, CancellationToken cancellationToken);
    Task<bool> CancelOrderAsync(MatchOrder orderToCancel, CancellationToken cancellationToken);
    Task<bool> MatchOrderAsync(MatchOrder order, CancellationToken cancellationToken);
    Task<bool> AddOrderAsync(MatchOrder order, CancellationToken cancellationToken);
}