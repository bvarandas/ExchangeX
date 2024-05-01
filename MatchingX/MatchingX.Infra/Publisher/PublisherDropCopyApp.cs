using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
namespace MatchingX.Infra.Publisher;

public class PublisherDropCopyApp : BackgroundService
{
    private readonly ILogger<PublisherDropCopyApp> _logger;
    private PublisherSocket _publisher;
    private readonly ConnectionZmq _config;
    private readonly IDropCopyCache _cache;
    public PublisherDropCopyApp(ILogger<PublisherDropCopyApp> logger, 
        IOptions<ConnectionZmq> options, IDropCopyCache cache )
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
        _publisher.Disconnect(_config.PublisherDropCopy.Uri);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher DropCopy ZeroMQ...");

        using (_publisher = new PublisherSocket())
        {
            _publisher.Bind(_config.PublisherDropCopy.Uri);

            while (!stoppingToken.IsCancellationRequested)
            {
                //var message = 
                
                //_publisher
                //    .SendMoreFrame(_config.Publisher.Topic[])
                //    .SendMultipartBytes();
                
                Thread.Sleep(10);
            }
        }
    }

    private void MessageTradeCaptureReport(TradeCaptureReport report)
    {

    }

    private void MessageExecutionReport( ExecutionReport report)
    {

    }
}
