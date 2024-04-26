using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Enums;
using SharedX.Core.Events;
namespace MacthingX.Application.Events;
public class OrderOpenedEvent: Event
{
    private readonly Order _order;
    public DateTime Timestamp { get; private set; }
    public OrderOpenedEvent(Order order )
    {
        _order = order;
        Timestamp = DateTime.Now;
    }
}