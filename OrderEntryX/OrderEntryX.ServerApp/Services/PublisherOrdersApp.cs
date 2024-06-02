using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEntryX.Core.Interfaces;
using SharedX.Core;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;

namespace OrderEntryX.Infra.Client;
public class PublisherOrdersApp : BackgroundService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<PublisherOrdersApp> _logger;
    private PushSocket _sender;
    private readonly IOrderEntryChache _orderEntryChache;
    private static Thread ThreadSenderOrder=null!;
    private readonly IOutboxBackgroundService<OrderEngine> _managerOutbox;
    public PublisherOrdersApp(ILogger<PublisherOrdersApp> logger, 
        IOptions<ConnectionZmq> options,
        IOrderEntryChache orderEntryChache,
        IOutboxBackgroundService<OrderEngine> managerOutbox)
    {
        _logger = logger;
        _config = options.Value;
        _orderEntryChache = orderEntryChache;
        _managerOutbox = managerOutbox;
    }
    public override Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o publisher de Orders ZeroMQ...");
        
        ThreadSenderOrder = new Thread( ()=>SenderOrder(stoppingToken));
        ThreadSenderOrder.Name = nameof(ThreadSenderOrder);
        ThreadSenderOrder.Start();
        
        return Task.CompletedTask;
    }

    private void SenderOrder(CancellationToken stoppingToken)
    {
        using (_sender = new PushSocket("@"+_config.OrderEntryToOrderEngine.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_orderEntryChache.TryDequeueOrderEntry(out OrderEngine order))
                {
                    var message = order.SerializeToByteArrayProtobuf<OrderEngine>();
                    _sender.SendMultipartBytes(message);

                    var envelope = new EnvelopeOutbox<OrderEngine>()
                    {
                        Id = order.OrderID,
                        Body = order,
                        LastTransaction = DateTime.Now,
                        ActivityOutbox = new ActivityOutbox()
                        {
                            Activity = OutboxActivities.OrderEntryToOrderEngineSent,
                            NextActivity = OutboxActivities.OrderEntryToOrderEngineSent
                        }
                    };

                    _managerOutbox.AddActivityAsync(envelope);
                }

                Thread.Sleep(10);
            }
        }
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher ZeroMQ...");
        _sender.Disconnect(_config.OrderEntryToOrderEngine.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finalizado!");
        await base.StopAsync(stoppingToken);
     }
}