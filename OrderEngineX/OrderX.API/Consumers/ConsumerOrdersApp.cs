using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEngineX.Application.Commands;
using OrderEngineX.Application.Commands.Order;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
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
    private readonly  IMatchingCache _cache;
    public ConsumerOrdersApp(ILogger<ConsumerOrdersApp> logger,
        IOptions<ConnectionZmq> options,
        IMediatorHandler mediator,
        IMatchingCache cache)
    {
        _logger = logger;
        _config = options.Value;
        _mediator = mediator;
        _cache = cache;
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
        bool isConnected = false;
        do
        {
            try
            {
                using (_receiver = new PullSocket(">"+_config.OrderEntryToOrderEngine.Uri))
                {
                    isConnected = true;
                    _logger.LogInformation("Receiver de ordens Conectado!!!");
                    
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveFrameBytes();
                        var order = message.DeserializeFromByteArrayProtobuf<OrderEngine>();
                        
                        SendOrderCommand(order);
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }

            Thread.Sleep(100);

        } while (!isConnected);
    }

    private void SendOrderCommand(OrderEngine order)
    {
        OrderEngineCommand command = null!;
        switch (order.Execution)
        {
            case Execution.ToCancel:
                command = new OrderCancelCommand(order, _cache);
                break;
            case Execution.ToCancelReplace:
                command = new OrderCancelReplaceCommand(order, _cache);
                break;
            case Execution.ToOpen:
                command = new OrderOpenedCommand(order, _cache);
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
