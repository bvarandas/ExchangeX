using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEngineX.Core.Interfaces;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
namespace OrderEngineX.API.Consumers;
public class ConsumerExecutionReportApp : BackgroundService
{
    private readonly ILogger<ConsumerExecutionReportApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private readonly IExecutionReportCache _cache;
    private static Thread ThreadReceiverExecutionReport = null!;
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
        _logger.LogInformation("Initializing receiver Execution Report ZeroMQ...");
        ThreadReceiverExecutionReport = new Thread(() => ReceiverExecutionReport(cancellationToken));
        ThreadReceiverExecutionReport.Name = nameof(ThreadReceiverExecutionReport);
        ThreadReceiverExecutionReport.Start();

        return Task.CompletedTask;
    }
    private void ReceiverExecutionReport(CancellationToken stoppingToken)
    {
        bool isConnected = false;
        do
        {
            try
            {
                _logger.LogInformation($"Receiver de DropCopy tentando conectar..{_config.MatchingToOrderEngine.Uri}");
                
                using (_receiver = new PullSocket(">"+_config.MatchingToOrderEngine.Uri))
                {
                    isConnected = true;
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveFrameBytes();
                        var execution = message.DeserializeFromByteArrayProtobuf<ExecutionReport>();
                        _cache.AddQueueExecutionReport(execution);
                        
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }
            Thread.Sleep(100);

        } while (!isConnected);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the publisher ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToOrderEngine.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finishing!");
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}