using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Match.Status;
public class MatchingCancelCommand : MatchingStatusEngineCommand
{
    public MatchingCancelCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}