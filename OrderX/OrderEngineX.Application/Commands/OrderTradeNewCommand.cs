using SharedX.Core.Commands;
using SharedX.Core.Models;
namespace MarketDataX.Application.Commands;
public class OrderTradeNewCommand : Command
{
    public OrderModel Order { get; set; }
    public DateTime Timestamp { get; private set; }
    public OrderTradeNewCommand(OrderModel Order)
    {
        this.Order = Order;
        Timestamp = DateTime.Now;
    }
}