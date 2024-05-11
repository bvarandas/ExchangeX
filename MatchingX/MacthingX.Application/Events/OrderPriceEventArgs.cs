using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Events;

public class OrderPriceEventArgs: EventArgs
{
    public OrderEngine Order {  get; private set; } 
    public OrderPriceEventArgs(OrderEngine order)
    {
        Order = order;
    }
}
