using OrderEntryX.Core.Entities;
using SharedX.Core.Commands;
using SharedX.Core.Matching;

namespace OrderEntryX.Application.Commands;
public class OrderCancelRequestCommand : Command
{
    public readonly OrderEntry Order;
        public DateTime Timestamp { get; private set; }
    public OrderCancelRequestCommand(OrderEntry order)
    {
        Timestamp = DateTime.Now;
        this.Order = order;
    }

    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}
