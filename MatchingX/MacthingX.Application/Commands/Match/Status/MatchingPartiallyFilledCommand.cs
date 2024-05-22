using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Match.Status;
public class MatchingPartiallyFilledCommand : MatchingStatusEngineCommand
{
    public MatchingPartiallyFilledCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}