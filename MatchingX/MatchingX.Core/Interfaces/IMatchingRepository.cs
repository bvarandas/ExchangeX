using FluentResults;
using MatchingX.Core.Entities;
namespace MatchingX.Core.Interfaces;
public interface IMatchingRepository
{
    Task<Result> UpsertOrderMatchingAsync(MatchOrder orders, CancellationToken cancellation);
    Task<Result> RemoveOrdersMatchingAsync(List<long> IdOrders, CancellationToken cancellation);
}