using MacthingX.Application.Commands.Order;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Match;
public class MatchingMarketCommand : MatchingEngineCommand
{
    public MatchingMarketCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        return true;
    }
}
