using FluentResults;
using SharedX.Core.Matching.OrderEngine;
namespace SharedX.Core.Interfaces;
public interface IBookOfferCache 
{
    Task<bool> UpsertBuyOrder(OrderEngine order);
    Task<bool> UpsertSellOrder(OrderEngine order);
    Task<Result<Dictionary<long, OrderEngine>>> GetBuyOrderBySymbol(string symbol);
    Task<Result<Dictionary<long, OrderEngine>>> GetSellOrderBySymbol(string symbol);
    Task<Result<OrderEngine>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<OrderEngine>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<bool> DeleteBuyOrderAsync(string symbol, long orderId);
    Task<bool> DeleteSellOrderAsync(string symbol, long orderId);
    Task<bool> DeleteAllOrderAsync(Dictionary<long, OrderEngine> order);
}