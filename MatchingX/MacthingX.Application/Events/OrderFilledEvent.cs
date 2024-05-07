using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;
public class OrderFilledEvent : Event
{
    private readonly OrderEngine _order;
    public DateTime Timestamp { get; private set; }
    public OrderFilledEvent(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        _order = order;
    }
}