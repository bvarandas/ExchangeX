using SharedX.Core.Matching;

namespace MatchingX.Core.Interfaces;
public  interface IMatch
{
    bool AddOrder(OrderEng order);
    bool ReplaceOrder(OrderEng order);
    bool CancelOrder(OrderEng orderToCancel);
    bool MatchOrder(OrderEng order);
}
