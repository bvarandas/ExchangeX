using SharedX.Core.Events;
using SharedX.Core.Models;

namespace OrderEngineX.Application.Events;
public class OrderTradeNewEvent: Event
{
    public readonly OrderModel Order;
    public DateTime Timestamp { get; private set; }
    public OrderTradeNewEvent(OrderModel order )
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}