using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Enums;
using SharedX.Core.Events;

namespace MacthingX.Application.Events;
public class OrderFilledEvent : Event
{
    private readonly OrderEng _order;
    public DateTime Timestamp { get; private set; }
    public OrderFilledEvent(OrderEng order)
    {
        Timestamp = DateTime.Now;
        _order = order;
    }
}