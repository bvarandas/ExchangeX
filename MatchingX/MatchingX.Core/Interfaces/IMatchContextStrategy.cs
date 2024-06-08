using SharedX.Core.Entities;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.Core.Interfaces;
public interface IMatchContextStrategy
{
    void SetStrategy(string strategyName);
    void ReceivedOrder(OrderEngine order);
}