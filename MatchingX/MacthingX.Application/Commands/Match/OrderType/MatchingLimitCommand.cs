using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Match.OrderType;
public class MatchingLimitCommand : MatchingEngineCommand
{
    public MatchingLimitCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        return true;
    }
}