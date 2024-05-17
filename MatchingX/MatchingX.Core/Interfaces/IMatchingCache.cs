using FluentResults;
using QuickFix;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public interface IMatchingCache 
{
    void UpsertBuyOrder(OrderEngine order);
    void UpsertSellOrder(OrderEngine order);
    Task<Result<Dictionary<long, OrderEngine>>> GetBuyOrderBySymbol(string symbol);
    Task<Result<Dictionary<long, OrderEngine>>> GetSellOrderBySymbol(string symbol);
    Task<Result<OrderEngine>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<OrderEngine>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<bool> DeleteBuyOrderAsync(string symbol, long orderId);
    Task<bool> DeleteSellOrderAsync(string symbol, long orderId);
    Task<bool> DeleteAllOrderAsync(Dictionary<long, OrderEngine> order);
}