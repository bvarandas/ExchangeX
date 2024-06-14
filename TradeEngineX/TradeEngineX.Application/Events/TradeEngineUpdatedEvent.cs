using SharedX.Core.Events;
using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Application.Events;
public class TradeEngineUpdatedEvent : Event 
{
    public readonly TradeReport TradeReport = null!;
    public TradeEngineUpdatedEvent(TradeReport tradeReport)
    {
        TradeReport = tradeReport;
    }
}
