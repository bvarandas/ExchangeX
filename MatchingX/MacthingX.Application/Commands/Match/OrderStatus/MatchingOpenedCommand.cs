using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Match.OrderStatus;
public class MatchingOpenedCommand : MatchingStatusEngineCommand
{
    public MatchingOpenedCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}
