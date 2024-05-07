using SharedX.Core.Commands;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;
public class OrderCanceledCommand : Command
{
    public readonly OrderEngine Order;
    public DateTime Timestamp { get; private set; }
    public OrderCanceledCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}