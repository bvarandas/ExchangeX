using MatchingX.Core.Entities;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;
public class OrderTradedEvent : Event
{
    public readonly MatchOrder Order;

    public OrderTradedEvent(MatchOrder order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}