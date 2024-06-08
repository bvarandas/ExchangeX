using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;

namespace OrderEngineX.Application.Commands.Order;

public class OrderFilledCommand : OrderEngineCommand
{
    private readonly IBookOfferCache _cache;
    public OrderFilledCommand(OrderEngine order, IBookOfferCache cache)
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
