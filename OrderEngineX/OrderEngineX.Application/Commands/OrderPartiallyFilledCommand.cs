using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEngineX.Application.Commands.Order;
public class OrderPartiallyFilledCommand : OrderEngineCommand
{
    private readonly IBookOfferCache _cache;
    public OrderPartiallyFilledCommand(OrderEngine order, IBookOfferCache cache)
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
