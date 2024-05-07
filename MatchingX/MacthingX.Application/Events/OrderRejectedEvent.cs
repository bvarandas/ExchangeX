using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;
public class OrderRejectedEvent : Event
{
    public readonly OrderEngine Order;
    public DateTime Timestamp { get; private set; }
    public OrderRejectedEvent(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}