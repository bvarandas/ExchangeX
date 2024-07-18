using MassTransit;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OrderEngineX.Core.Interfaces;
using ServiceStack;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using SharedX.Core.ValueObjects;

namespace OrderEngineX.API.Consumers;
public class ConsumerExecutionReportApp : IConsumer<EnvelopeOutbox<ExecutionReport>>, IHostedService
{
    private readonly ILogger<ConsumerExecutionReportApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private readonly IExecutionReportCache _cache;
    private static Thread ThreadReceiverExecutionReport = null!;
    private readonly IOutboxBackgroundService<ExecutionReport> _outboxBackgroundService = null!;
    public ConsumerExecutionReportApp(ILogger<ConsumerExecutionReportApp> logger
        , IOptions<ConnectionZmq> options
        , IExecutionReportCache cache
        , IOutboxBackgroundService<ExecutionReport> outboxBackgroundService
        , IBus bus) 
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
        _outboxBackgroundService = outboxBackgroundService;
    }
    public Task StartAsync(CancellationToken cancellationToken)
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
                        
                        var deleted = _outboxBackgroundService.DeleteOutboxCacheAsync(execution, execution.ExecID);

                        if (deleted.Result.IsSuccess)
                        {
                            _cache.UpsertExecutionReportAsync(execution);
                        }
                            
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

    

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the publisher ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToOrderEngine.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finishing!");

        return Task.CompletedTask;
    }

    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<EnvelopeOutbox<ExecutionReport>> context)
    {
        var execution = context.Message.Body;

        var deleted = _outboxBackgroundService.DeleteOutboxCacheAsync(execution, execution.ExecID);

        if (deleted.IsSuccess())
        {
            _cache.UpsertExecutionReportAsync(execution);
        }
        return Task.CompletedTask;
    }
}