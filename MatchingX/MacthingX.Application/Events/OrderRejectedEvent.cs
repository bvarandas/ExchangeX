using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;
public class OrderRejectedEvent : Event
{
    public readonly Order Order;
    public DateTime Timestamp { get; private set; }
    public OrderRejectedEvent(Order order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}