using OrderEntryX.Core.Entities;
using SharedX.Core.Commands;
using SharedX.Core.Matching;

namespace OrderEntryX.Application.Commands;

public class OrderCancelReplaceRequestCommand : Command
{
    public readonly OrderEntry Order;

    public DateTime Timestamp { get; private set; }
    public OrderCancelReplaceRequestCommand(OrderEntry order)
    {
        Timestamp = DateTime.Now;
        this.Order = order;
    }
}
