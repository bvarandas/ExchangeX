using Microsoft.Extensions.Options;
using NetMQ.Sockets;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Specs;
using NetMQ;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.OrderEngine;

namespace OrderEngineX.API.Publishers;
public class PublisherOrderApp : BackgroundService
{
    private readonly ILogger<PublisherOrderApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly IOrderEngineCache _cache;
    private static Thread ThreadSenderOrder = null!;
    public PublisherOrderApp(ILogger<PublisherOrderApp> logger,
        IOptions<ConnectionZmq> options,
        IOrderEngineCache cache)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        ThreadSenderOrder = new Thread(() => SenderOrder(cancellationToken));
        ThreadSenderOrder.Name = nameof(ThreadSenderOrder);
        ThreadSenderOrder.Start();

        return Task.CompletedTask;
    }

    private void SenderOrder(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initializing the Publisher Orders ZeroMQ...");

        using (_sender = new PushSocket("@"+_config.OrderEngineToMatching.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueOrder(out OrderEngine order))
                {
                    var message = order.SerializeToByteArrayProtobuf<OrderEngine>();
                    _sender.SendMultipartBytes(message);
                }
                Thread.Sleep(10);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the publisher ZeroMQ...");
        _sender.Disconnect(_config.OrderEngineToMatching.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finishing!");
        return base.StopAsync(cancellationToken);
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}