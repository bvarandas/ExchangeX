using MatchingX.Core.Entities;
using SharedX.Core.Commands;

namespace MacthingX.Application.Commands;
public class ExecutedTradeCommand : Command
{

    public readonly MatchingEngine ExecutedTrades;
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeCommand(MatchingEngine executedTrade)
    {
        Timestamp = DateTime.Now;
        ExecutedTrades = executedTrade;
    }

    public override bool IsValid()
    {
        return true; //não é necessário essa validação
    }
}