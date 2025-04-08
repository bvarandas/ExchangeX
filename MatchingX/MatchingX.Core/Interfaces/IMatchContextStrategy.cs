using MatchingX.Core.Entities;
namespace MatchingX.Core.Interfaces;
public interface IMatchContextStrategy
{
    bool SetStrategy(string strategyName);
    void ReceivedOrder(MatchingEngine order, CancellationToken cancellationToken);
}