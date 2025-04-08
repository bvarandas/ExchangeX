using MatchingX.Core.Entities;
using SharedX.Core.Commands;
namespace MacthingX.Application.Commands.Match;
public abstract class MatchingEngineCommand : MatchCommand
{
    public MatchOrder Order { get; protected set; } = new MatchOrder();
    public DateTime Timestamp { get; protected set; } = DateTime.Now;
}

public abstract class MatchingStatusEngineCommand : Command
{
    public MatchOrder Order { get; protected set; } = new MatchOrder();
    public DateTime Timestamp { get; protected set; } = DateTime.Now;
}