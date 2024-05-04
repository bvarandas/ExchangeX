using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
namespace MatchingX.Infra.Publisher;
public class PublisherMarketDataApp : BackgroundService
{
    private readonly ILogger<PublisherMarketDataApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly IMarketDataCache _cache;
    private readonly Semaphore _semaphore;
    public PublisherMarketDataApp(ILogger<PublisherMarketDataApp> logger,
        IOptions<ConnectionZmq> options, IMarketDataCache cache)
    {
        _semaphore = new Semaphore(1, 2, "prioridadeIncremental");
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
        _logger.LogInformation("Iniciando o Publisher MarketData ZeroMQ...");
        _sender.Disconnect(_config.PublisherMarketData.Uri);
        return base.StopAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher MarketData ZeroMQ...");
        using (_sender = new PushSocket(_config.PublisherMarketData.Uri))
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
}