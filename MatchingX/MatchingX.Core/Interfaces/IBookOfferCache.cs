using FluentResults;
using MatchingX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Matching.MarketData;

namespace MatchingX.Core.Interfaces;
public interface IBookOfferCache
{
    Task<Result<MatchOrder>> GetOrderByIdandSymbolAsync(long orderId, string symbol, SideTrade side);
    Task<Result<Dictionary<long, MatchOrder>>> GetOrderBySymbolAsync(string symbol, SideTrade side);
    Task<bool> UpsertOrder(MatchOrder order);
    Task<bool> DeleteOrderAsync(string symbol, long orderId, SideTrade side);
    Task<bool> DeleteAllOrderAsync(Dictionary<long, MatchOrder> dicOrders);
    Task<Result> UpsertPrice(MarketData marketData);
}