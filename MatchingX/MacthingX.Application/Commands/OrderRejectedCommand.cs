using SharedX.Core.Matching;
using SharedX.Core.Commands;
namespace MacthingX.Application.Events;
public class OrderRejectedCommand : Command
{
    public readonly Order Order;
    public DateTime Timestamp { get; private set; }
    public OrderRejectedCommand(Order order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}