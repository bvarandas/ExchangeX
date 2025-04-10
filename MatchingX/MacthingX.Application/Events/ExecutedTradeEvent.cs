using MatchingX.Core.Entities;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;
public class ExecutedTradeEvent : Event
{
    public MatchingEngine ExecutedOrders { get; set; }
    public DateTime Timestamp { get; private set; }
    public ExecutedTradeEvent(MatchingEngine executedOrders)
    {
        ExecutedOrders = executedOrders;
        Timestamp = DateTime.Now;
    }
}