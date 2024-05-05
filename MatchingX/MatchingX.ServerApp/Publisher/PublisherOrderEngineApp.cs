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
 public class PublisherOrderEngineApp : BackgroundService
 {
    private readonly ILogger<PublisherOrderEngineApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly IDropCopyCache _cache;
    private static Thread TreadSenderDropCopy=null!;
    public PublisherOrderEngineApp(ILogger<PublisherOrderEngineApp> logger,
        IOptions<ConnectionZmq> options,
        IDropCopyCache cache)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender DropCopy ZeroMQ...");

        TreadSenderDropCopy = new Thread(() => SenderDropCopy(cancellationToken));
        TreadSenderDropCopy.Name = nameof(TreadSenderDropCopy);
        TreadSenderDropCopy.Start();

        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o Sender DropCopy ZeroMQ...");
        _sender.Disconnect(_config.MatchingToOrderEngine.Uri);
        return base.StopAsync(cancellationToken);
    }

    private void SenderDropCopy(CancellationToken stoppingToken)
    {
        using (_sender = new PushSocket(_config.MatchingToOrderEngine.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueExecuteToOrderReport(out ExecutionReport report))
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