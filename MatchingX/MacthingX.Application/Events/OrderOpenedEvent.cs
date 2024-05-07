using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;
public class OrderOpenedEvent: Event
{
    public readonly OrderEngine Order;
    public DateTime Timestamp { get; private set; }
    public OrderOpenedEvent(OrderEngine order )
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}