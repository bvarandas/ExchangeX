using MediatR;
namespace MacthingX.Application.Events;
public class ExecutionReportHandler : 
    INotificationHandler<OrderFilledCommand>,
    INotificationHandler<OrderOpenedEvent>,
    INotificationHandler<OrderRejectedEvent>,
    INotificationHandler<OrderCanceledEvent>
{
    public ExecutionReportHandler()
    {

    }

    public Task Handle(OrderFilledCommand notification, CancellationToken cancellationToken)
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

    public Task Handle(OrderCanceledEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
