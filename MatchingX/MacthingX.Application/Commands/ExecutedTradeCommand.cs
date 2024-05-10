using SharedX.Core.Commands;
using SharedX.Core.Matching.DropCopy;

namespace MacthingX.Application.Commands;
public class ExecutedTradeCommand : Command
{
    public readonly (TradeCaptureReport, TradeCaptureReport) ExecutedTrades;
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeCommand((TradeCaptureReport, TradeCaptureReport) executedTrade)
    {
        Timestamp = DateTime.Now;
        ExecutedTrades = executedTrade;
    }

    public override bool IsValid()
    {
        return true; //não é necessário essa validação
    }
}