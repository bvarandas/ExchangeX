using MatchingX.Core.Entities;
using SharedX.Core.Events;
namespace MacthingX.Application.Events;
public class OrderModifiedEvent : Event
{
    public readonly MatchOrder Order;
    public OrderModifiedEvent(MatchOrder order)
    {
        Order = order;
        Timestamp = DateTime.Now;
    }
}