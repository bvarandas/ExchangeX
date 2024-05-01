using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Enums;
using SharedX.Core.Events;
namespace MacthingX.Application.Events;
public class OrderCanceledEvent : Event
{
    public readonly Order Order;
    public DateTime Timestamp { get; private set; }
    public OrderCanceledEvent(Order order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}