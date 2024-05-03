using OrderEngineX.Application.Events;
using MediatR;

namespace OrderEngineX.Application.Events;

public class OrderTradeEventHandler :
    IRequestHandler<OrderTradeNewEvent, bool>,
    IRequestHandler<OrderTradeModifyEvent, bool>,
    IRequestHandler<OrderTradeCancelEvent, bool>
{
    public OrderTradeEventHandler()
    {

    }
    public Task<bool> Handle(OrderTradeNewEvent request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Handle(OrderTradeModifyEvent request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Handle(OrderTradeCancelEvent request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
