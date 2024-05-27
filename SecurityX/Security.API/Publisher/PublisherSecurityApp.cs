using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Specs;
namespace Security.API.Publisher;
public class PublisherSecurityApp : BackgroundService
{
    private readonly ILogger<PublisherSecurityApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly ISecurityCache _cache;
    private static Thread TreadSenderMarketData = null!;
    public PublisherSecurityApp(ILogger<PublisherSecurityApp> logger,
        IOptions<ConnectionZmq> options, ISecurityCache cache)
    {
        //_semaphore = new Semaphore(1, 2, "prioridadeIncremental");
        _logger = logger;
        _config = options.Value;
        _cache = cache;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Securities ZeroMQ...");

        TreadSenderMarketData = new Thread(() => SenderMarketData(cancellationToken));
        TreadSenderMarketData.Name = nameof(TreadSenderMarketData);
        TreadSenderMarketData.Start();

        return base.StartAsync(cancellationToken);
    }

    private void SenderMarketData(CancellationToken stoppingToken)
    {
        using (_sender = new PushSocket("@"+_config.SecurityToMarketData.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueSecurity(out var marketData))
                {
                    var message = marketData.SerializeToByteArrayProtobuf<SecurityEngine>();
                    _sender.SendMultipartBytes(message);
                }
                Thread.Sleep(10);
            }
        }
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o Sender Securities ZeroMQ...");
        _sender.Disconnect(_config.SecurityToMarketData.Uri);
        return base.StopAsync(cancellationToken);
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
