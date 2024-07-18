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
using Sharedx.Infra.Outbox.Services;
using MassTransit;
using ServiceStack;
using SharedX.Core.ValueObjects;

namespace OrderEngineX.API.Consumers;
public class ConsumerOrdersApp : IConsumer<EnvelopeOutbox<OrderEngine>>, IHostedService
{
    private readonly ILogger<ConsumerOrdersApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private static Thread ThreadReceiverOrder = null!;
    private readonly IMediatorHandler _mediator;
    private readonly IBookOfferCache _cache;
    private readonly IOutboxBackgroundService<OrderEngine> _outboxBackgroundService = null!;
    public ConsumerOrdersApp(ILogger<ConsumerOrdersApp> logger,
        IOptions<ConnectionZmq> options,
        IMediatorHandler mediator,
        IBookOfferCache cache,
        IOutboxBackgroundService<OrderEngine> outboxBackgroundService,
        IBus bus) 
    {
        _logger = logger;
        _config = options.Value;
        _mediator = mediator;
        _cache = cache;
        _outboxBackgroundService = outboxBackgroundService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inicializando o receiver de Orders do ZeroMQ...");
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
                        
                        var deleted = _outboxBackgroundService.DeleteOutboxCacheAsync(order, order.OrderID);
                        if (deleted.IsSuccess())
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

    private bool SendOrderCommand(OrderEngine order)
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
        
        var result = _mediator.Send(command);
        return result.Result.IsSuccess;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o consumer de ordens do zeroMQ...");
        _receiver.Disconnect(_config.OrderEntryToOrderEngine.Uri);
        _logger.LogInformation("Consumer ZeroMq...Finalizado!");
        return Task.CompletedTask;
    }

    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<EnvelopeOutbox<OrderEngine>> context)
    {
        var order = context.Message.Body;

        var deleted = _outboxBackgroundService.DeleteOutboxCacheAsync(order, order.OrderID);
        
        if (deleted.IsSuccess())
            SendOrderCommand(order);

        return Task.CompletedTask;
    }
}