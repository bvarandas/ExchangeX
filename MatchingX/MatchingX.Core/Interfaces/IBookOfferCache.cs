using FluentResults;
using MatchingX.Core.Entities;

namespace MatchingX.Core.Interfaces;
public interface IBookOfferCache
{
    Task<Result<MatchOrder>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<MatchOrder>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<Dictionary<long, MatchOrder>>> GetBuyOrderBySymbol(string symbol);
    Task<Result<Dictionary<long, MatchOrder>>> GetSellOrderBySymbol(string symbol);
    Task<bool> UpsertBuyOrder(MatchOrder order);
    Task<bool> UpsertSellOrder(MatchOrder order);
    Task<bool> DeleteBuyOrderAsync(string symbol, long orderId);
    Task<bool> DeleteSellOrderAsync(string symbol, long orderId);
    Task<bool> DeleteAllOrderAsync(Dictionary<long, MatchOrder> dicOrders);
}