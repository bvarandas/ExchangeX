using OrderEntryX.Core.Entities;
using SharedX.Core.Commands;
namespace OrderEntryX.Application.Commands;
public class NewOrderSingleCommand : Command
{
    public readonly OrderEntry Order;
    
    public DateTime Timestamp { get; private set; }
    public NewOrderSingleCommand(OrderEntry order)
    {
        Timestamp = DateTime.Now;
        this.Order = order;
    }
}
