using FluentResults;
using MediatR;
using SharedX.Core.Commands;
using SharedX.Core.Enums;
using SharedX.Core.Events;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Querys;
using System.Collections;

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

    public Task<(OrderStatus, Dictionary<long, OrderEngine>)> SendMatchCommand<T>(T command)
        where T : MatchCommand
        //where R : (OrderStatus, Dictionary<long, OrderEngine>)
    {
        var result = _mediator.Send(command).Result;
        return Task.FromResult( result);

    }

    //public Task<R> SendMatchCommand<T, R>(T command)
    //    where T : MatchCommand
    //    where R : Tuple<Enum, Dictionary<long, OrderEngine>>
    //{
    //    var medi = _mediator.Send(command);
    //    return Task.FromResult( medi.Result);
    //}

    //public Task<R> SendMatchCommand<T, R>(T command) where T : MatchCommand R : 
    //{
    //    var medi = _mediator.Send(command);
    //    return medi;
    //}



    //public Task<R> SendCommand<Q, R>(Q command)  where Q : MatchCommand
    //    where R : Tuple<Enum, Dictionary<long, OrderEngine>>
    //{
    //    return _mediator.Send(command);
    //}


}
