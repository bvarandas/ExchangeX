using MediatR;
using SharedX.Core.Commands;
using SharedX.Core.Events;
namespace SharedX.Core.Bus;
public class InMemmoryBus : IMediatorHandler
{
    private readonly IMediator _mediator;
    

    public InMemmoryBus( IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task SendCommand<T>(T command) where T : Command
    {
        return _mediator.Send(command);
    }

    public Task RaiseEvent<T>(T @event) where T : Event
    {
        //if (!@event.MessageType.Equals("DomainNotification"))
        //{
        //    _EventStore.Save(@event);
        //}


        return _mediator.Publish(@event);
    }
}
