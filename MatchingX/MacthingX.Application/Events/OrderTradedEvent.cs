using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;
public class OrderTradedEvent : Event
{
    public readonly OrderEngine Order;

    public OrderTradedEvent(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}