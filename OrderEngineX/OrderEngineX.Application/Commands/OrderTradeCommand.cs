using OrderEngineX.Application.Commands;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace MacthingX.Application.Commands.Order;
public class OrderTradeCommand : OrderEngineCommand
{
    private readonly IBookOfferCache _cache;
    public OrderTradeCommand(OrderEngine order, IBookOfferCache cache)
    {
        Timestamp = DateTime.Now;
        Order = order;
        _cache = cache;
    }
    public override bool IsValid()
    {
        return true;
    }
}
