using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
namespace MatchingX.ServerApp.Publisher;
public class PublisherMarketDataApp : BackgroundService
{
    private readonly ILogger<PublisherMarketDataApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly IMatchingCache _cache;
    private readonly Semaphore _semaphore;
    private static Thread TreadSenderMarketData = null!;
    public PublisherMarketDataApp(ILogger<PublisherMarketDataApp> logger,
        IOptions<ConnectionZmq> options, IMatchingCache cache)
    {
        //_semaphore = new Semaphore(1, 2, "prioridadeIncremental");
        _logger = logger;
        _config = options.Value;
        _cache = cache;
    }

    private void SenderMarketData(CancellationToken stoppingToken)
    {
        using (_sender = new PushSocket("@"+_config.MatchingToMarketData.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueMarketData(out var marketData))
                {
                    var message = marketData.SerializeToByteArrayProtobuf<MarketData>();
                    _sender.SendMultipartBytes(message);
                }

                Thread.Sleep(10);
            }
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Marketdata ZeroMQ...");

        TreadSenderMarketData = new Thread(() => SenderMarketData(cancellationToken));
        TreadSenderMarketData.Name = nameof(TreadSenderMarketData);
        TreadSenderMarketData.Start();

        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o Sender MarketData ZeroMQ...");
        _sender.Disconnect(_config.MatchingToMarketData.Uri);
        return base.StopAsync(cancellationToken);
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}