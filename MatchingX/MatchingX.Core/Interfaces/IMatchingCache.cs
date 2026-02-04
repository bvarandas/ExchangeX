using FluentResults;
using MatchingX.Core.Entities;
using SharedX.Core.Enums;
namespace MatchingX.Core.Interfaces;
public interface IMatchingCache
{
    Task<Result> UpsertOrderMatchingAsync(MatchingEngine matchEngine, CancellationToken cancellation);
    Task<Result<Dictionary<long, MatchOrder>>> GetBuyOrderBySymbol(string symbol);
    Task<Result<Dictionary<long, MatchOrder>>> GetSellOrderBySymbol(string symbol);
    Task<Result<bool>> RemoveOrderMatchingAsync(string symbol, long orderId, SideTrade side);
}