using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Extensions;
using SharedX.Core.Matching;
using SharedX.Core.Specs;
using System.Collections.Concurrent;

namespace OrderEngineX.Infra.Publisher;

public class PublisherOrderApp : BackgroundService, IPublisherOrderApp
{
    private readonly ILogger<PublisherOrderApp> _logger;
    private PublisherSocket _publisher;
    private readonly ConnectionZmq _config;
    private ConcurrentQueue<Order> _orderQueue;

    public PublisherOrderApp(ILogger<PublisherOrderApp> logger,
        IOptions<ConnectionZmq> options)
    {
        _orderQueue = new ConcurrentQueue<Order>();
        _logger = logger;
        _config = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _publisher = new PublisherSocket();
        _publisher.Bind(_config.PublisherOrders.Uri);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _publisher.Disconnect(_config.PublisherOrders.Uri);

        return base.StopAsync(cancellationToken);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher Order ZeroMQ...");
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_orderQueue.TryDequeue(out var order))
            {
                var message = order.SerializeToByteArrayProtobuf<Order>();
                _publisher
                    .SendMoreFrame(_config.PublisherOrders.Topics[1])
                    .SendMultipartBytes(message);
            }
            Thread.Sleep(5000);
        }
    }

    public Task AddOrderToQueue(Order order)
    {
        _orderQueue.Enqueue(order);
        return Task.CompletedTask;
    }
}
