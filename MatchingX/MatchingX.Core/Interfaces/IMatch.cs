using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public  interface IMatch
{
    string Name { get; }
    void ReceiveOrder(OrderEngine order);
    bool ModifyOrder(OrderEngine order);
    bool CancelOrder(OrderEngine orderToCancel);
    Task<bool> MatchOrderAsync(OrderEngine order);
}