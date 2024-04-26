using SharedX.Core.Matching;

namespace MatchingX.Core.Interfaces;
public  interface IMatch
{
    bool AddOrder(Order order);
    bool ReplaceOrder(Order order);
    bool CancelOrder(Order orderToCancel);
    bool MatchOrder(Order order);
}
