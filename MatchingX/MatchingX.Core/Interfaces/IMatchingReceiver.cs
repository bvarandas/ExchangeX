using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;

namespace MatchingX.Core.Interfaces;
public interface IMatchingReceiver
{
    void ReceiveOrder(OrderEngine order);
}