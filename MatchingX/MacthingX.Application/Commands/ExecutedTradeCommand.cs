using SharedX.Core.Commands;
using SharedX.Core.Matching;
namespace MacthingX.Application.Commands;
public class ExecutedTradeCommand : Command
{
    public readonly ExecutedTrade ExecutedTrade;
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeCommand(ExecutedTrade executedTrade)
    {
        Timestamp = DateTime.Now;
        ExecutedTrade = executedTrade;
    }
}