using FluentResults;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public interface IMatchingCache 
{
    void AddBuyOrder(OrderEngine order);
    void AddSellOrder(OrderEngine order);
    void UpdateBuyOrder(OrderEngine order);
    void UpdateSellOrder(OrderEngine order);
    Task<Result<Dictionary<long, OrderEngine>>> GetBuyOrderBySymbol(string symbol);
    Task<Result<Dictionary<long, OrderEngine>>> GetSellOrderBySymbol(string symbol);
    Task<Result<OrderEngine>> GetBuyOrderByIdandSymbolAsync(long orderId, string symbol);
    Task<Result<OrderEngine>> GetSellOrderByIdandSymbolAsync(long orderId, string symbol);
    bool TryGetBuyOrders(string symbol, out Dictionary<long, OrderEngine> dic);
    bool TryGetSellOrders(string symbol, out Dictionary<long, OrderEngine> dic);
}