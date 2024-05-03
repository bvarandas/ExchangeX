using SharedX.Core.Events;
using SharedX.Core.Models;
namespace OrderEngineX.Application.Events;
public class OrderTradeCancelEvent : Event
{
    public readonly OrderModel Order;
    public DateTime Timestamp { get; private set; }
    public OrderTradeCancelEvent(OrderModel order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}
