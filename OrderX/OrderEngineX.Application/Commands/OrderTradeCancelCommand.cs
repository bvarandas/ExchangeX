using SharedX.Core.Commands;
using SharedX.Core.Models;

namespace MarketDataX.Application.Commands;
public class OrderTradeCancelCommand : Command
{
    public OrderModel Order { get; set; }
    public DateTime Timestamp { get; private set; }
    public OrderTradeCancelCommand(OrderModel orderModel )
    {
        Order = orderModel;
        Timestamp = DateTime.Now;
    }
}
