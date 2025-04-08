using FluentResults;
using MatchingX.Core.Entities;
namespace MatchingX.Core.Interfaces;

public interface IOrderStopCache
{
    Task<bool> DeleteBuyOrderAsync(string symbol, long orderId);
    Task<bool> DeleteSellOrderAsync(string symbol, long orderId);
    Task UpsertBuyOrderAsync(MatchOrder order);
    Task UpsertSellOrderAsync(MatchOrder order);
    Task<Result<MatchOrder>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<MatchOrder>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<Dictionary<long, MatchOrder>>> GetBuyOrderBySymbolAsync(string symbol);
    Task<Result<Dictionary<long, MatchOrder>>> GetSellOrderBySymbolAsync(string symbol);
}