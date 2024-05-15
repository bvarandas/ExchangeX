using SharedX.Core.Commands;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Commands.Match;

public abstract class MatchingEngineCommand : MatchCommand
{
    public OrderEngine Order { get; protected set; } = new OrderEngine();
    public DateTime Timestamp { get; protected set; } = DateTime.Now;
}
