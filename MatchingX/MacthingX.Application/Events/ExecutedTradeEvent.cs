using MatchingX.Core;
using MediatR;
using SharedX.Core.Events;
using SharedX.Core.Matching.DropCopy;

namespace MacthingX.Application.Events;
public class ExecutedTradeEvent : Event
{
    public (TradeCaptureReport,TradeCaptureReport) ExecutedTrade {  get; private set; }
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeEvent((TradeCaptureReport, TradeCaptureReport) executedTrade)
    {
        ExecutedTrade = executedTrade;
        Timestamp = DateTime.Now;
    }
}