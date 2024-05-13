using MassTransit;
using SharedX.Core.Enums;
using SharedX.Core.Bus;
using SharedX.Core.Matching.OrderEngine;
using OrderEngineX.Application.Commands;
using MarketDataX.Application.Commands;

namespace OrderX.API.Consumers;
public class ConsumerOrdersBusApp : IConsumer<OrderEngine>
{
    private readonly ILogger<ConsumerOrdersBusApp> _logger;
    private readonly IMediatorHandler _mediatorHandler;
    public ConsumerOrdersBusApp(ILogger<ConsumerOrdersBusApp> logger, IMediatorHandler mediatorHandler)
    {
        _logger = logger;
        _mediatorHandler = mediatorHandler;
    }

    public Task Consume(ConsumeContext<OrderEngine> context)
    {
        OrderEngineCommand command = null!;
        switch (context.Message.Execution)
        {
            case Execution.ToCancel:
                command = new OrderTradeCancelCommand(context.Message);
                break;
            case Execution.ToCancelReplace:
                command = new OrderTradeCancelReplaceCommand(context.Message);
                break;
            case Execution.ToOpen:
                command = new OrderTradeNewCommand(context.Message);
                break;
        }
        _mediatorHandler.SendCommand(command);
        
        return Task.CompletedTask;
    }
}