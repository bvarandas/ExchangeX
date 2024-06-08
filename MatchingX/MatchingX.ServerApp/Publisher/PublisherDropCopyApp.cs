using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
namespace MatchingX.ServerApp.Publisher;
public class PublisherDropCopyApp : BackgroundService
{
    private readonly ILogger<PublisherDropCopyApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly IMatchingCache _cache;
    private static Thread ThreadSenderReport = null!;
    public PublisherDropCopyApp(ILogger<PublisherDropCopyApp> logger, 
        IOptions<ConnectionZmq> options,
        IMatchingCache cache )
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Report ZeroMQ...");

        ThreadSenderReport = new Thread(() => SenderReport(cancellationToken));
        ThreadSenderReport.Name = nameof(ThreadSenderReport);
        ThreadSenderReport.Start();
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _sender.Disconnect(_config.MatchingToDropCopy.Uri);
        return base.StopAsync(cancellationToken);
    }

    private void SenderReport(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher DropCopy ZeroMQ...");

        using (_sender = new PushSocket(_config.MatchingToDropCopy.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueExecuteReport(out ExecutionReport report))
                {
                    var message = report.SerializeToByteArrayProtobuf<ExecutionReport>();
                    _sender.SendMultipartBytes(message);
                }

                Thread.Sleep(10);
            }
        }
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}