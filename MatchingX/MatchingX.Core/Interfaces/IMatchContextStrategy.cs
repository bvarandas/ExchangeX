using SharedX.Core.Matching.OrderEngine;

namespace MatchingX.Core.Interfaces;
public interface IMatchContextStrategy
{
    void SetStrategy(IMatch match);
    void ReceivedOrder(OrderEngine order);
}
