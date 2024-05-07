using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public  interface IMatch
{
    bool AddOrder(OrderEngine order);
    bool ReplaceOrder(OrderEngine order);
    bool CancelOrder(OrderEngine orderToCancel);
    bool MatchOrder(OrderEngine order);
}
