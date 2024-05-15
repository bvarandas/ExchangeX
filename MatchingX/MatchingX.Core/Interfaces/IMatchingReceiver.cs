using SharedX.Core.Entities;

using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public interface IMatchingReceiver
{
    void ReceiveOrder(OrderEngine order);
    void ReceiveSecurity(SecurityEngine security);
}