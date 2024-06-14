using SharedX.Core.Events;
using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Application.Events;
public class TradeEngineRemovedEvent : Event
{
    public readonly TradeReport TradeReport = null!;
    public TradeEngineRemovedEvent(TradeReport tradeReport)
    {
        TradeReport = tradeReport;
    }
}