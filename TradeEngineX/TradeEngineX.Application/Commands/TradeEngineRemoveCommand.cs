using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Application.Commands;
public class TradeEngineRemoveCommand : TradeEngineCommand
{
    public TradeEngineRemoveCommand(TradeReport tradeReport, CancellationToken cancellationToken)
    {
        TradeReport = tradeReport;
        CancellationToken = cancellationToken;
    }
    public override bool IsValid() => true;
}