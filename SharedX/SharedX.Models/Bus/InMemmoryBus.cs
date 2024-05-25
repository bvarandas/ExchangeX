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
    public InMemmoryBus(IMediator mediator)
    {
        _mediator = mediator;
    }
    public Task SendCommand<T>(T command) where T : Command
    {
        return _mediator.Send(command);
    }
    public Task RaiseEvent<T>(T @event) where T : Event
    {
        return _mediator.Publish(@event);
    }
    public Task<(OrderStatus, Dictionary<long, OrderEngine>)> SendMatchCommand<T>(T command)
        where T : MatchCommand
        //where R : (OrderStatus, Dictionary<long, OrderEngine>)
    {
        var result = _mediator.Send(command).Result;
        return Task.FromResult( result);

    }
}