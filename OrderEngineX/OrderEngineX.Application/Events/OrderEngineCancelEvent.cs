using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Events;
public class OrderEngineCancelEvent : Event
{
    public readonly OrderEngine Order;
    public DateTime Timestamp { get; private set; }
    public OrderEngineCancelEvent(OrderEngine order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}