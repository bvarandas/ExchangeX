using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Enums;
using SharedX.Core.Events;
namespace MacthingX.Application.Events;
public class OrderOpenedEvent: Event
{
    public readonly OrderEng Order;
    public DateTime Timestamp { get; private set; }
    public OrderOpenedEvent(OrderEng order )
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}