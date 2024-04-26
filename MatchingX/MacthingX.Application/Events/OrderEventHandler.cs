using MediatR;

namespace MacthingX.Application.Events;

public class OrderEventHandler :    
    INotificationHandler<OrderCanceledEvent>,
    INotificationHandler<OrderFilledEvent>,
    INotificationHandler<OrderOpenedEvent>,
    INotificationHandler<OrderRejectedEvent>

{
    public OrderEventHandler() { }
    public Task Handle(OrderCanceledEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Handle(OrderFilledEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Handle(OrderOpenedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Handle(OrderRejectedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
