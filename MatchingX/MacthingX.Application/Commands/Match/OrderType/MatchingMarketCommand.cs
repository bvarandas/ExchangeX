using MatchingX.Core.Entities;
namespace MacthingX.Application.Commands.Match.OrderType;
public class MatchingMarketCommand : MatchingEngineCommand
{
    public MatchingMarketCommand(MatchOrder order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        return true;
    }
}
