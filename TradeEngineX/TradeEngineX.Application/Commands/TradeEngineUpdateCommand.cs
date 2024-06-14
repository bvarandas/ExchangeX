using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Application.Commands;
public class TradeEngineUpdateCommand : TradeEngineCommand
{
    public TradeEngineUpdateCommand(TradeReport tradeReport, CancellationToken cancellationToken)
    {
        TradeReport  = tradeReport;
        CancellationToken = cancellationToken;
    }
    public override bool IsValid() =>true;
}