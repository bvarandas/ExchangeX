using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Match.OrderType;
public class MatchingStopLimitCommand : MatchingEngineCommand
{
    public MatchingStopLimitCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        return true;
    }
}
