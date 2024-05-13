using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public  interface IMatch
{
    void ReceiveOrder(OrderEngine order);
    bool AddOrder(OrderEngine order);
    bool ReplaceOrder(OrderEngine order);
    bool ModifyOrder(OrderEngine order);
    bool CancelOrder(OrderEngine orderToCancel);
    Task<bool> MatchBuyOrderAsync(OrderEngine order);
    Task<bool> MatchSellOrderAsync(OrderEngine order);
}
