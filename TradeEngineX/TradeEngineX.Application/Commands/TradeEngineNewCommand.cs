using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Application.Commands;
public class TradeEngineNewCommand : TradeEngineCommand
{
    public TradeEngineNewCommand(TradeReport tradeReport, CancellationToken cancellationToken)
    {
        TradeReport = tradeReport;
        CancellationToken = cancellationToken;
    }
    public override bool IsValid() => true;
}