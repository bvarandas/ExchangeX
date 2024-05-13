using MarketDataX.Application.Commands;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEngineX.Application.Commands;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
namespace OrderEngineX.API.Consumers;
public class ConsumerOrdersApp : BackgroundService
{
    private readonly ILogger<ConsumerOrdersApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private static Thread ThreadReceiverOrder = null!;
    private readonly IMediatorHandler _mediator;
    public ConsumerOrdersApp(ILogger<ConsumerOrdersApp> logger,
        IOptions<ConnectionZmq> options,
        IMediatorHandler mediator)
    {
        _logger = logger;
        _config = options.Value;
        _mediator = mediator;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing o receiver Orders ZeroMQ...");
        ThreadReceiverOrder = new Thread(() => ReceiverOrders(cancellationToken));
        ThreadReceiverOrder.Name = nameof(ThreadReceiverOrder);
        ThreadReceiverOrder.Start();

        return Task.CompletedTask;
    }
    private void ReceiverOrders(CancellationToken stoppingToken)
    {
        using (_receiver = new PullSocket(_config.OrderEntryToOrderEngine.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _receiver.ReceiveFrameBytes();
                var order = message.DeserializeFromByteArrayProtobuf<OrderEngine>();
                SendOrderCommand(order);
                Thread.Sleep(10);
            }
        }
    }

    private void SendOrderCommand(OrderEngine order)
    {
        OrderEngineCommand command = null!;
        switch (order.Execution)
        {
            case Execution.ToCancel:
                command = new OrderTradeCancelCommand(order);
                break;
            case Execution.ToCancelReplace:
                command = new OrderTradeCancelReplaceCommand(order);
                break;
            case Execution.ToOpen:
                command = new OrderTradeNewCommand(order);
                break;
        }
        _mediator.SendCommand(command);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the publisher ZeroMQ...");
        _receiver.Disconnect(_config.OrderEntryToOrderEngine.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finishing!");
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
