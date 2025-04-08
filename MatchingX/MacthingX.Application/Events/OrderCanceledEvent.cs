using MatchingX.Core.Entities;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;
public class OrderCanceledEvent : Event
{
    public readonly MatchOrder Order;
    public DateTime Timestamp { get; private set; }
    public OrderCanceledEvent(MatchOrder order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}