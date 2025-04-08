using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public interface IMatchingReceiver
{
    Task ReceiveOrder(OrderEngine order, CancellationToken cancellationToken);
    //void ReceiveSecurity(SecurityEngine security);
}