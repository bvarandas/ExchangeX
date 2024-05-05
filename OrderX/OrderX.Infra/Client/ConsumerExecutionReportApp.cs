using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
namespace OrderEngineX.Infra.Publisher;
public class ConsumerExecutionReportApp : BackgroundService
{
    private readonly ILogger<ConsumerExecutionReportApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private readonly IExecutionReportCache _cache;
    public ConsumerExecutionReportApp(ILogger<ConsumerExecutionReportApp> logger,
        IOptions<ConnectionZmq> options,
        IExecutionReportCache cache)
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
        _receiver.Disconnect(_config.OrderEntryToOrderEngine.Uri);
        return base.StopAsync(cancellationToken);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Publisher Order ZeroMQ...");
        using ( _receiver = new PullSocket(_config.OrderEntryToOrderEngine.Uri)) 
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _receiver.ReceiveFrameBytes();
                var execution = message.DeserializeFromByteArrayProtobuf<ExecutionReport>();
                _cache.AddQueueExecutionReport(execution);
                Thread.Sleep(10);
            }
        }
    }

    
}
