using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Enums;
using SharedX.Core.Commands;

namespace MacthingX.Application.Events;
public class OrderFilledCommand : Command
{
    public readonly Order Order;
    public DateTime Timestamp { get; private set; }
    public OrderFilledCommand(Order order)
    {
        Timestamp = DateTime.Now;
        Order = order;
    }
}