using SharedX.Core.Events;
using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Application.Events;
public class TradeEngineCreatedEvent :Event
{
    public readonly TradeReport TradeReport = null!;
    public TradeEngineCreatedEvent(TradeReport tradeReport)
    {
        TradeReport = tradeReport;
    }
}