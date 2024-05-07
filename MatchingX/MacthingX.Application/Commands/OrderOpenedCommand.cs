using SharedX.Core.Commands;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Events;
public class OrderOpenedCommand : Command
{
    public readonly OrderEngine Order;
    public DateTime Timestamp { get; private set; }
    public OrderOpenedCommand(OrderEngine order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}