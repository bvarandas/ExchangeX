using MatchingX.Core.Entities;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;
public class OrderOpenedEvent : Event
{
    public readonly MatchOrder Order;
    public OrderOpenedEvent(MatchOrder order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}