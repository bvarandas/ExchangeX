using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;
public class OrderRejectedEvent : Event
{
    private readonly Order _order;
    public DateTime Timestamp { get; private set; }
    public OrderRejectedEvent(Order order)
    {
        Timestamp = DateTime.Now;
        _order = order;
    }
}