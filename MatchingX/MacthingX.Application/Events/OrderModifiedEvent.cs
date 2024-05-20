using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Events;
public class OrderModifiedEvent : Event
{
    public readonly OrderEngine Order;
    public OrderModifiedEvent(OrderEngine order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}