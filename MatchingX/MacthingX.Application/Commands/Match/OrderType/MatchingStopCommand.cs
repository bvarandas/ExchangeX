using MatchingX.Core.Entities;
namespace MacthingX.Application.Commands.Match.OrderType;
public class MatchingStopCommand : MatchingEngineCommand
{
    public MatchingStopCommand(MatchOrder order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        return true;
    }
}