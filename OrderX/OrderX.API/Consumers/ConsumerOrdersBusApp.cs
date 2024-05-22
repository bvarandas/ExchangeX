using MassTransit;
using SharedX.Core.Enums;
using SharedX.Core.Bus;
using SharedX.Core.Matching.OrderEngine;
using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Interfaces;
namespace OrderX.API.Consumers;
public class ConsumerOrdersBusApp : IConsumer<OrderEngine>
{
    private readonly ILogger<ConsumerOrdersBusApp> _logger;
    private readonly IMediatorHandler _mediatorHandler;
    private readonly IMatchingCache _cache;
    public ConsumerOrdersBusApp(
        ILogger<ConsumerOrdersBusApp> logger, 
        IMediatorHandler mediatorHandler,
        IMatchingCache cache
        )
    {
        _cache = cache;
        _logger = logger;
        _mediatorHandler = mediatorHandler;
    }

    public Task Consume(ConsumeContext<OrderEngine> context)
    {
        OrderEngineCommand command = null!;
        switch (context.Message.Execution)
        {
            case Execution.ToCancel:
                command = new OrderCancelCommand(context.Message,_cache);
                break;
            case Execution.ToCancelReplace:
                command = new OrderCancelReplaceCommand(context.Message, _cache);
                break;
            case Execution.ToOpen:
                command = new OrderOpenedCommand(context.Message, _cache);
                break;
        }
        _mediatorHandler.SendCommand(command);
        
        return Task.CompletedTask;
    }
}