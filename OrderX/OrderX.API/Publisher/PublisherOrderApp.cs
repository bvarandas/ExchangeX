using Microsoft.Extensions.Options;
using NetMQ.Sockets;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Specs;
using NetMQ;
using SharedX.Core.Matching;
using SharedX.Core.Extensions;
namespace OrderEngineX.API.Publisher;
public class PublisherOrderApp : BackgroundService
{
    private readonly ILogger<PublisherOrderApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly IOrderEngineCache _cache;
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
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _sender.Disconnect(_config.PublisherDropCopy.Uri);
        return base.StopAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher DropCopy ZeroMQ...");

        using (_sender = new PushSocket(_config.ConsumerExecutionReport.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueOrder(out Order order))
                {
                    var message = order.SerializeToByteArrayProtobuf<Order>();
                    _sender.SendMultipartBytes(message);
                }
                Thread.Sleep(10);
            }
        }
    }
}