using MatchingX.Core;
using MediatR;
using SharedX.Core.Events;
using SharedX.Core.Matching;
namespace MacthingX.Application.Events;
public class ExecutedTradeEvent : Event
{
    public ExecutedTrade ExecutedTrade {  get; private set; }
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeEvent(ExecutedTrade executedTrade)
    {
        ExecutedTrade = executedTrade;
        Timestamp = DateTime.Now;
    }
}