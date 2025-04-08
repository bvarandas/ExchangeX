using MatchingX.Core.Entities;
namespace MacthingX.Application.Commands.Match.OrderType;
public class MatchingStopLimitCommand : MatchingEngineCommand
{
    public MatchingStopLimitCommand(MatchOrder order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        return true;
    }
}
