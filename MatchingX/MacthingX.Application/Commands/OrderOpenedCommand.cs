using SharedX.Core.Matching;
using SharedX.Core.Commands;
namespace MacthingX.Application.Events;
public class OrderOpenedCommand : Command
{
    public readonly Order Order;
    public DateTime Timestamp { get; private set; }
    public OrderOpenedCommand(Order order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}