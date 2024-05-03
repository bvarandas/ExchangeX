using SharedX.Core.Commands;
using SharedX.Core.Enums;
using SharedX.Core.Models;

namespace OrderEngineX.Application.Commands;
public class OrderTradeModifyCommand : Command
{
    public OrderModel Order { get; set; }
    public DateTime Timestamp { get; private set; }
    public OrderTradeModifyCommand(OrderModel Order)
    {
        this.Order = Order;
        Timestamp = DateTime.Now;
    }
}
