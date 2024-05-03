using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ.Sockets;
using SharedX.Core.Specs;
using NetMQ;
using SharedX.Core.Matching.MarketData;
using System.Threading;

namespace MatchingX.Infra.Publisher;

public class PublisherMarketDataApp : BackgroundService
{
    private readonly ILogger<PublisherMarketDataApp> _logger;
    private PublisherSocket _publisher;
    private readonly ConnectionZmq _config;
    private readonly IMarketDataCache _cache;
    private Thread ThreadSecurities;
    private Thread ThreadSnapshotIncremental;
    private Thread ThreadIncremental;
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
        _publisher = new PublisherSocket();
        _publisher.Bind(_config.PublisherMarketData.Uri);

        ThreadSecurities = new Thread(()=>MessageSecurity(cancellationToken));
        ThreadSecurities.Name = nameof(MessageSecurity);
        ThreadSecurities.Start();

        ThreadSnapshotIncremental  = new Thread(()=>MessageSnapshotIncremental(cancellationToken));
        ThreadSnapshotIncremental.Name = nameof(MessageSnapshotIncremental);
        ThreadSnapshotIncremental.Start();

        ThreadSnapshotIncremental = new Thread(()=>MessageIncremental(cancellationToken));
        ThreadSnapshotIncremental.Name = nameof(MessageIncremental);
        ThreadSnapshotIncremental.Start();

        return base.StartAsync(cancellationToken);
    }
        


    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _publisher.Disconnect(_config.PublisherMarketData.Uri);

        return base.StopAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher MarketData ZeroMQ...");
        while (!stoppingToken.IsCancellationRequested)
        {
                
            Thread.Sleep(5000);
        }
    }
    

    private void MessageSecurity(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {


            Thread.Sleep(10);
        }
        //whie
        //var message =
        //        _publisher
        //            .SendMoreFrame(_config.PublisherMarketData.Topic[0])
        //            .SendMultipartBytes();
    }

    private void MessageSnapshotIncremental(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {

            Thread.Sleep(10);
        }
    }

    private void MessageIncremental(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {

            Thread.Sleep(10);
        }
    }
}