using FluentResults;
using MatchingX.Core.Entities;
namespace MatchingX.Core.Interfaces;
public interface IMatchingCache
{
    Task<Result> UpsertBuyOrderMatchingAsync(MatchingEngine matchEngine, CancellationToken cancellation);
    Task<Result> UpsertSellOrderMatchingAsync(MatchingEngine matchEngine, CancellationToken cancellation);
    Task<Result<Dictionary<long, MatchOrder>>> GetBuyOrderBySymbol(string symbol);
    Task<Result<Dictionary<long, MatchOrder>>> GetSellOrderBySymbol(string symbol);
    Task<Result<bool>> RemoveOrderMatchingAsync(string symbol, long orderId);

}