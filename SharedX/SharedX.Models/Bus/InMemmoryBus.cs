using MediatR;
using SharedX.Core.Commands;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;
namespace SharedX.Core.Bus;
public class InMemmoryBus : IMediatorHandler
{
    private readonly IMediator _mediator;
    public InMemmoryBus(IMediator mediator)
    {
        _mediator = mediator;
    }
    public Task<(OrderStatus, Dictionary<long, OrderEngine>)> SendMatchCommand<T>(T command)
        where T : MatchCommand
        //where R : (OrderStatus, Dictionary<long, OrderEngine>)
    {
        var result = _mediator.Send(command).Result;
        return Task.FromResult( result);

    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return _mediator.Send(request, cancellationToken);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        return _mediator.Send(request, cancellationToken);
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        return _mediator.Send(request, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return _mediator.CreateStream(request, cancellationToken);
        
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return _mediator.CreateStream(request, cancellationToken);
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return _mediator.Publish(notification, cancellationToken);
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return _mediator.Publish(notification, cancellationToken);
    }
}