using SharedX.Core.Commands;
using SharedX.Core.Matching.DropCopy;

namespace MacthingX.Application.Commands;
public class ExecutedTradeCommand : Command
{
    public readonly Dictionary<long,DropCopyReport> ExecutedTrades;
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeCommand(Dictionary<long, DropCopyReport> executedTrade)
    {
        Timestamp = DateTime.Now;
        ExecutedTrades = executedTrade;
    }

    public override bool IsValid()
    {
        return true; //não é necessário essa validação
    }
}