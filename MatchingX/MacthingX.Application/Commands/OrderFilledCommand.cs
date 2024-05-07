using SharedX.Core.Commands;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;
public class OrderFilledCommand : Command
{
    public readonly OrderEngine Order;
    public DateTime Timestamp { get; private set; }
    public OrderFilledCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}