using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Specs;

namespace OrderEngineX.API.Publishers;

public class PublisherOrderReportApp : BackgroundService
{
    private readonly ILogger<PublisherOrderReportApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly IOrderReportCache _cache;
    private static Thread ThreadSenderOrderReport = null!;

    public PublisherOrderReportApp(ILogger<PublisherOrderReportApp> logger,
        IOptions<ConnectionZmq> options,
        IOrderReportCache cache)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        ThreadSenderOrderReport = new Thread(() => SenderOrderResponses(cancellationToken));
        ThreadSenderOrderReport.Name = nameof(ThreadSenderOrderReport);
        ThreadSenderOrderReport.Start();

        return Task.CompletedTask;
    }

    private void SenderOrderResponses(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initializing the Publisher Orders ZeroMQ...");

        using (_sender = new PushSocket("@"+_config.OrderEngineToMatching.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueReport(out ReportFix report))
                {
                    var message = report.SerializeToByteArrayProtobuf<ReportFix>();
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
